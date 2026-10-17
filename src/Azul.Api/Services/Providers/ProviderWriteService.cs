using Azul.Api.Common.Exceptions;
using Azul.Api.Data;
using Azul.Api.DTOs.Providers;
using Azul.Api.Entities;
using Azul.Api.Services.Images;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Azul.Api.Services.Providers;

// Escrituras de proveedores: alta, edición y baja, con sus controles contra la base.
public class ProviderWriteService(
    AppDbContext dbContext,
    IPhoneHasher phoneHasher,
    IConfiguration configuration,
    IImageStorage imageStorage,
    ImageProcessor imageProcessor,
    ILogger<ProviderWriteService> logger) : IProviderWriteService
{
    private const string InstagramUniqueIndex = "IX_Providers_InstagramUsername";
    private const string ClientIdUniqueIndex = "IX_Providers_ClientId";
    private const int MaxProvidersPerPhone = 5;

    private async Task EnsurePhoneUnderLimit(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return;
        }
        var providersDb = await dbContext.Providers.Where(p => p.PhoneNumber == phoneNumber).Take(MaxProvidersPerPhone).CountAsync();
        
        if (providersDb >= MaxProvidersPerPhone)
        {
            throw new ConflictException("phone_limit_reached", "Este celular ya tiene el máximo de fichas.",
                "phoneNumber");
        }
        
    }
    // En la base va la key; al front se le devuelve la URL armada con la dirección base.
    private readonly string photoBaseUrl = configuration["Photos:BaseUrl"]
        ?? throw new InvalidOperationException("Falta la configuración Photos:BaseUrl.");

    // PUT /api/providers/{clientId}/photo
    public async Task SavePendingPhotoAsync(Guid clientId, Stream photo, CancellationToken ct)
    {
        var provider = await dbContext.Providers
                           .Include(p => p.Photos.Where(ph => ph.Status == ProviderPhotoStatus.Pending))
                           .FirstOrDefaultAsync(p => p.ClientId == clientId, ct)
                       ?? throw new NotFoundException("provider", clientId);

        var processed = await imageProcessor.ProcessAsync(photo, ct);
        var baseKey = $"providers/{provider.Id}/{Guid.NewGuid():N}";
        var key = $"{baseKey}.webp";

        await imageStorage.SaveAsync(key, processed.Normal, "image/webp", ct);
        await imageStorage.SaveAsync($"{baseKey}-128.webp", processed.Thumb, "image/webp", ct);

        var replaced = provider.Photos.ToList();
        dbContext.ProviderPhotos.RemoveRange(replaced);
        provider.Photos.Add(new ProviderPhoto { Key = key, CreatedAt = DateTime.UtcNow });
        provider.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        foreach (var old in replaced)
        {
            await DeletePhotoFilesAsync(old.Key, ct);
        }
    }

    private async Task DeletePhotoFilesAsync(string key, CancellationToken ct)
    {
        await imageStorage.DeleteAsync(key, ct);
        await imageStorage.DeleteAsync(key.Replace(".webp", "-128.webp"), ct);
    }

    // POST /api/providers
    public async Task<ProviderDto> CreateAsync(ProviderSaveDto providerSaveDto)
    {
        ProviderSaveDtoNormalizer.Normalize(providerSaveDto);

        await EnsureNotAlreadyCreated(providerSaveDto.ClientId);

        var offeringIds = providerSaveDto.OfferingIds.Distinct().ToList();
        await EnsureOfferingsExist(offeringIds);
        await EnsureInstagramIsFree(providerSaveDto.InstagramUsername);
        await EnsurePhoneUnderLimit(providerSaveDto.PhoneNumber);

        var now = DateTime.UtcNow;
        var provider = new Provider
        {
            ClientId = providerSaveDto.ClientId,
            DisplayName = string.Empty,
            PhoneNumber = string.Empty,
            Status = ProviderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            Offerings = offeringIds.Select(id => new ProviderOffering { OfferingId = id, CreatedAt = now }).ToList()
        };
        ApplyFields(provider, providerSaveDto);

        // Se guarda junto con el Provider: un solo SaveChanges, una sola transacción.
        provider.Consents.Add(new ProviderConsent
        {
            PhoneHash = phoneHasher.Hash(provider.PhoneNumber),
            AcceptedAt = now
        });

        dbContext.Add(provider);
        await SaveChangesCatchingDuplicates();

        // Solo ids y tipo: sin celular ni nombre.
        logger.LogInformation("Alta creada: Id={ProviderId}, Tipo={ProviderType}", provider.Id, provider.Type);

        return await LoadByIdAsync(provider.Id);
    }

    // PUT /api/providers/{id}
    public async Task UpdateAsync(int id, ProviderSaveDto providerSaveDto)
    {
        ProviderSaveDtoNormalizer.Normalize(providerSaveDto);

        var provider = await dbContext.Providers
                           .Include(p => p.Offerings)
                           .FirstOrDefaultAsync(p => p.Id == id)
                       ?? throw new NotFoundException("provider", id);

        var offeringIds = providerSaveDto.OfferingIds.Distinct().ToList();
        await EnsureOfferingsExist(offeringIds);
        await EnsureInstagramIsFree(providerSaveDto.InstagramUsername, excludeId: id);

        var now = DateTime.UtcNow;
        ApplyFields(provider, providerSaveDto);
        provider.UpdatedAt = now;

        provider.Offerings.RemoveAll(po => !offeringIds.Contains(po.OfferingId));
        var currentIds = provider.Offerings.Select(po => po.OfferingId).ToList();
        provider.Offerings.AddRange(offeringIds
            .Except(currentIds)
            .Select(offeringId => new ProviderOffering { OfferingId = offeringId, CreatedAt = now }));

        await SaveChangesCatchingDuplicates();
    }

    // DELETE /api/providers/{id}
    public async Task DeleteAsync(int id)
    {
        var photoKeys = await dbContext.Providers
            .Where(p => p.Id == id)
            .Select(p => new { p.PhotoKey, Keys = p.Photos.Select(ph => ph.Key).ToList() })
            .FirstOrDefaultAsync();

        if (photoKeys is null)
        {
            throw new NotFoundException("provider", id);
        }

        await dbContext.Providers
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();

        foreach (var key in photoKeys.Keys.Append(photoKeys.PhotoKey).OfType<string>().Distinct())
        {
            await DeletePhotoFilesAsync(key, CancellationToken.None);
        }

        logger.LogInformation("Ficha {ProviderId} borrada", id);
    }

    private const string InvalidStatusTransitionCode = "invalid_status_transition";

    // POST /api/admin/providers/{id}/publish: se puede publicar desde Pending o Suspended.
    public async Task PublishAsync(int id)
    {
        var providerDb = await GetProviderWithPhotosAsync(id);

        if (providerDb.Status is not (ProviderStatus.Pending or ProviderStatus.Suspended))
        {
            var message = GetInvalidStatusMessage("publicar", providerDb.Status);
            throw new ConflictException(InvalidStatusTransitionCode, message);
        }

        var now = DateTime.UtcNow;
        var replacedPhotoKeys = new List<string>();
        var pendingPhoto = providerDb.Photos.FirstOrDefault(ph => ph.Status == ProviderPhotoStatus.Pending);
        if (pendingPhoto is not null)
        {
            var replaced = providerDb.Photos.Where(ph => ph.Status == ProviderPhotoStatus.Approved).ToList();
            dbContext.ProviderPhotos.RemoveRange(replaced);
            replacedPhotoKeys.AddRange(replaced.Select(ph => ph.Key));
            if (providerDb.PhotoKey is not null)
            {
                replacedPhotoKeys.Add(providerDb.PhotoKey);
            }

            pendingPhoto.Status = ProviderPhotoStatus.Approved;
            pendingPhoto.ReviewedAt = now;
            providerDb.PhotoKey = pendingPhoto.Key;
        }

        providerDb.Status = ProviderStatus.Published;
        providerDb.FirstPublishedAt ??= now;
        providerDb.UpdatedAt = now;
        await dbContext.SaveChangesAsync();

        foreach (var key in replacedPhotoKeys.Distinct())
        {
            await DeletePhotoFilesAsync(key, CancellationToken.None);
        }
        logger.LogInformation("Ficha {ProviderId} publicada", id);
    }

    // POST /api/admin/providers/{id}/suspend: solo se suspenden perfiles Published.
    public async Task SuspendAsync(int id)
    {
        var providerDb = await GetProviderByIdAsync(id);

        if (providerDb.Status != ProviderStatus.Published)
        {
            var message = GetInvalidStatusMessage("suspender", providerDb.Status);
            throw new ConflictException(InvalidStatusTransitionCode, message);
        }

        providerDb.Status = ProviderStatus.Suspended;
        providerDb.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Ficha {ProviderId} suspendida", id);
    }

    // POST /api/admin/providers/{id}/reject: solo se rechazan perfiles Pending.
    public async Task RejectAsync(int id)
    {
        var providerDb = await GetProviderWithPhotosAsync(id);

        if (providerDb.Status != ProviderStatus.Pending)
        {
            var message = GetInvalidStatusMessage("rechazar", providerDb.Status);
            throw new ConflictException(InvalidStatusTransitionCode, message);
        }

        var now =  DateTime.UtcNow;
        var discardedPhoto = providerDb.Photos.FirstOrDefault(ph => ph.Status == ProviderPhotoStatus.Pending);
        if (discardedPhoto is not null)
        {
            discardedPhoto.Status = ProviderPhotoStatus.Rejected;
            discardedPhoto.ReviewedAt = now;
        }

        providerDb.Status = ProviderStatus.Rejected;
        providerDb.UpdatedAt = now;
        await dbContext.SaveChangesAsync();

        if (discardedPhoto is not null)
        {
            await DeletePhotoFilesAsync(discardedPhoto.Key, CancellationToken.None);
        }
        logger.LogInformation("Ficha {ProviderId} rechazada", id);
    }

    private async Task<Provider> GetProviderWithPhotosAsync(int id)
    {
        return await dbContext.Providers.Include(p => p.Photos).FirstOrDefaultAsync(p => p.Id == id)
               ?? throw new NotFoundException("provider", id);
    }

    private async Task<Provider> GetProviderByIdAsync(int id)
    {
        return await dbContext.Providers.FirstOrDefaultAsync(p => p.Id == id)
               ?? throw new NotFoundException("provider", id);
    }

    private static string GetInvalidStatusMessage(string action, ProviderStatus status)
    {
        return $"No se puede {action} un provider con status: {status}";
    }

    // Sin filtrar por estado: para devolver el proveedor recién creado (queda Pending).
    private async Task<ProviderDto> LoadByIdAsync(int id)
    {
        return await dbContext.Providers
                   .Where(p => p.Id == id)
                   .Select(ProviderProjections.ToDto(photoBaseUrl))
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException("provider", id);
    }

    // Copia los datos del request (ya normalizado y validado) al Provider.
    private static void ApplyFields(Provider provider, ProviderSaveDto dto)
    {
        var type = dto.Type!.Value;
        var isIndividual = type == ProviderType.Individual;

        provider.Type = type;
        provider.FirstName = isIndividual ? dto.FirstName : null;
        provider.LastName = isIndividual ? dto.LastName : null;
        provider.DisplayName = isIndividual ? $"{dto.FirstName} {dto.LastName}" : dto.DisplayName!;
        provider.Description = dto.Description;
        provider.PhotoKey = dto.PhotoKey;
        provider.PhoneNumber = dto.PhoneNumber!;
        provider.InstagramUsername = dto.InstagramUsername;
        provider.Address = dto.Address;
        provider.Hours = dto.Hours;
    }

    private async Task EnsureInstagramIsFree(string? instagramUsername, int? excludeId = null)
    {
        if (instagramUsername is null)
        {
            return;
        }

        if (await dbContext.Providers.AnyAsync(p => p.Id != excludeId && p.InstagramUsername == instagramUsername))
        {
            throw DuplicateInstagram();
        }
    }

    // Si dos requests con el mismo Instagram o el mismo ClientId pasan la consulta a la vez, el índice único ataja al segundo.
    private async Task SaveChangesCatchingDuplicates()
    {
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           {
                                               SqlState: PostgresErrorCodes.UniqueViolation,
                                               ConstraintName: InstagramUniqueIndex
                                           })
        {
            throw DuplicateInstagram();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           {
                                               SqlState: PostgresErrorCodes.UniqueViolation,
                                               ConstraintName: ClientIdUniqueIndex
                                           })
        {
            throw AlreadyCreated();
        }
    }

    private async Task EnsureNotAlreadyCreated(Guid? clientId)
    {
        if (clientId is not null && await dbContext.Providers.AnyAsync(p => p.ClientId == clientId))
        {
            throw AlreadyCreated();
        }
    }

    private static ConflictException AlreadyCreated()
        => new("already_created", "Esta ficha ya fue creada.");

    private static ConflictException DuplicateInstagram()
        => new("duplicate_instagram", "Ya hay un proveedor con ese usuario de Instagram.", "instagramUsername");

    private async Task EnsureOfferingsExist(List<int> offeringIds)
    {
        var existingIds = await dbContext.Offerings
            .Where(o => offeringIds.Contains(o.Id))
            .Select(o => o.Id)
            .ToListAsync();

        var missing = offeringIds.Except(existingIds).ToList();
        if (missing.Count > 0)
        {
            throw ValidationFailedException.ForField("offeringIds", $"No existen los servicios: {string.Join(", ", missing)}.");
        }
    }
    
    
}
