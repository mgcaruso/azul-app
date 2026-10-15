using Azul.Api.Common.Exceptions;
using Azul.Api.Data;
using Azul.Api.DTOs.Providers;
using Azul.Api.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Azul.Api.Services.Providers;

// Escrituras de proveedores: alta, edición y baja, con sus controles contra la base.
public class ProviderWriteService(AppDbContext dbContext, IPhoneHasher phoneHasher, IConfiguration configuration)
    : IProviderWriteService
{
    private const string InstagramUniqueIndex = "IX_Providers_InstagramUsername";

    // En la base va la key; al front se le devuelve la URL armada con la dirección base.
    private readonly string photoBaseUrl = configuration["Photos:BaseUrl"]
        ?? throw new InvalidOperationException("Falta la configuración Photos:BaseUrl.");

    // POST /api/providers
    public async Task<ProviderDto> CreateAsync(ProviderSaveDto providerSaveDto)
    {
        ProviderSaveDtoNormalizer.Normalize(providerSaveDto);

        var offeringIds = providerSaveDto.OfferingIds.Distinct().ToList();
        await EnsureOfferingsExist(offeringIds);
        await EnsureInstagramIsFree(providerSaveDto.InstagramUsername);

        var now = DateTime.UtcNow;
        var provider = new Provider
        {
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
        await SaveChangesCatchingDuplicateInstagram();

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

        await SaveChangesCatchingDuplicateInstagram();
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

    // Si dos requests con el mismo Instagram pasan la consulta a la vez, el índice único ataja al segundo.
    private async Task SaveChangesCatchingDuplicateInstagram()
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
    }

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
