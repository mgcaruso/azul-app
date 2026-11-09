using Azul.Api.Common.Exceptions;
using Azul.Api.Data;
using Azul.Api.DTOs.Providers;
using Azul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Azul.Api.Services.Providers;

// Escrituras de proveedores: alta, edición y baja, con sus controles contra la base.
public class ProviderWriteService(
    AppDbContext dbContext,
    IPhoneHasher phoneHasher,
    IConfiguration configuration,
    ILogger<ProviderWriteService> logger) : IProviderWriteService
{
    private const string InstagramUniqueIndex = "IX_ProviderContacts_InstagramUsername";
    private const string UserIdUniqueIndex = "IX_Providers_UserId";
    // En la base va la key; al front se le devuelve la URL armada con la dirección base.
    private readonly string photoBaseUrl = configuration["Photos:BaseUrl"]
        ?? throw new InvalidOperationException("Falta la configuración Photos:BaseUrl.");

    // POST /api/providers
    public async Task<ProviderDto> CreateAsync(Guid userId, ProviderSaveDto providerSaveDto)
    {
        ProviderSaveDtoNormalizer.Normalize(providerSaveDto);

        var offeringIds = providerSaveDto.OfferingIds.Distinct().ToList();

        await EnsureHasNoProvider(userId);
        await EnsureOfferingsExist(offeringIds);
        await EnsureInstagramIsFree(providerSaveDto.InstagramUsername);

        var now = DateTime.UtcNow;
        var provider = new Provider
        {
            DisplayName = string.Empty,
            Contact = new ProviderContact(),
            Status = ProviderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            Offerings = offeringIds.Select(id => new ProviderOffering { OfferingId = id, CreatedAt = now }).ToList(),
            UserId = userId
        };
        ApplyFields(provider, providerSaveDto);

        // Se guarda junto con el Provider: un solo SaveChanges, una sola transacción.
        provider.Consents.Add(new ProviderConsent
        {
            // Se firma con el teléfono que tenga: el WhatsApp o, si solo cargó fijo, el fijo.
            PhoneHash = phoneHasher.Hash(provider.Contact.PhoneNumber ?? provider.Contact.CallPhone!),
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
                           .Include(p => p.Contact)
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
        var deletedRows = await dbContext.Providers
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new NotFoundException("provider", id);
        }

        logger.LogInformation("Ficha {ProviderId} borrada", id);
    }

    // PUT /api/me/provider: the owner edits their ficha. Published stays published and Pending stays Pending.
    // Suspended and Rejected are read-only for the owner.
    public async Task UpdateMineAsync(Guid userId, ProviderSaveDto providerSaveDto)
    {
        var mine = await GetMineIdAndStatusAsync(userId);

        if (mine.Status is not (ProviderStatus.Pending or ProviderStatus.Published))
        {
            throw new ConflictException(InvalidStatusTransitionCode, GetInvalidStatusMessage("editar", mine.Status));
        }

        await UpdateAsync(mine.Id, providerSaveDto);
    }

    // DELETE /api/me/provider: the owner removes their ficha. Allowed in any status (right to erasure, ley 25.326).
    public async Task DeleteMineAsync(Guid userId)
    {
        var mine = await GetMineIdAndStatusAsync(userId);
        await DeleteAsync(mine.Id);
    }

    private async Task<(int Id, ProviderStatus Status)> GetMineIdAndStatusAsync(Guid userId)
    {
        var mine = await dbContext.Providers
            .Where(p => p.UserId == userId)
            .Select(p => new { p.Id, p.Status })
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("provider", userId);

        return (mine.Id, mine.Status);
    }

    private const string InvalidStatusTransitionCode = "invalid_status_transition";

    // POST /api/admin/providers/{id}/publish: se puede publicar desde Pending o Suspended.
    public async Task PublishAsync(int id)
    {
        var providerDb = await GetProviderByIdAsync(id);

        if (providerDb.Status is not (ProviderStatus.Pending or ProviderStatus.Suspended))
        {
            var message = GetInvalidStatusMessage("publicar", providerDb.Status);
            throw new ConflictException(InvalidStatusTransitionCode, message);
        }

        var now = DateTime.UtcNow;
        providerDb.Status = ProviderStatus.Published;
        providerDb.FirstPublishedAt ??= now;
        providerDb.UpdatedAt = now;
        await dbContext.SaveChangesAsync();
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
        var providerDb = await GetProviderByIdAsync(id);

        if (providerDb.Status != ProviderStatus.Pending)
        {
            var message = GetInvalidStatusMessage("rechazar", providerDb.Status);
            throw new ConflictException(InvalidStatusTransitionCode, message);
        }

        var now =  DateTime.UtcNow;
        providerDb.Status = ProviderStatus.Rejected;
        providerDb.UpdatedAt = now;
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Ficha {ProviderId} rechazada", id);
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
        provider.Contact.PhoneNumber = dto.PhoneNumber;
        provider.Contact.CallPhone = dto.CallPhone;
        provider.Contact.InstagramUsername = dto.InstagramUsername;
        provider.Contact.ExtraUrl = dto.ExtraUrl;
        provider.Address = dto.Address;
        provider.Hours = dto.Hours;
    }

    private async Task EnsureInstagramIsFree(string? instagramUsername, int? excludeId = null)
    {
        if (instagramUsername is null)
        {
            return;
        }

        if (await dbContext.ProviderContacts.AnyAsync(c => c.ProviderId != excludeId && c.InstagramUsername == instagramUsername))
        {
            throw DuplicateInstagram();
        }
    }

    // Los índices únicos atajan lo que las consultas previas no ven cuando llegan dos pedidos a la vez.
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
                                               ConstraintName: UserIdUniqueIndex
                                           })
        {
  
            logger.LogWarning("Alta duplicada atajada por el índice {Index}", UserIdUniqueIndex);
            throw ProviderAlreadyExists();
        }
    }

    private static ConflictException DuplicateInstagram()
        => new("duplicate_instagram", "Ya hay un proveedor con ese usuario de Instagram.", "instagramUsername");

    private static ConflictException ProviderAlreadyExists()
        => new("provider.already_exists", "Ya tenés una ficha creada.");

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

    private async Task EnsureHasNoProvider(Guid userId)
    {
        var hasProvider = await dbContext.Providers.AnyAsync(p => p.UserId == userId);
        if (hasProvider)
        {
            throw ProviderAlreadyExists();
        }
    }
    
    
}
