using Azul.Api.Common.Exceptions;
using Azul.Api.Common.Security;
using Azul.Api.DTOs.Administrators;
using Azul.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace Azul.Api.Services.Administrators;

public class AdministratorService(UserManager<AppUser> userManager) : IAdministratorService
{
    public async Task<List<AdministratorDto>> GetAllAsync()
    {
        var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
        return admins
            .OrderBy(u => u.DisplayName)
            .Select(ToDto)
            .ToList();
    }
    
    private static AdministratorDto ToDto(AppUser user) =>
        new(user.Id, user.Email!, user.DisplayName, user.PictureUrl);
    
    public async Task AddAsync(string email) {
        var userDb =await userManager.FindByEmailAsync(email);
        if (userDb is null)
        {
            throw new NotFoundException("user",email);
        }
        
        var isAdmin = await userManager.IsInRoleAsync(userDb, AppRoles.Admin);
        if (isAdmin)
        {
            throw new ConflictException("admin.already_admin", "User is already an admin");
        }

        
        var result = await userManager.AddToRoleAsync(userDb, AppRoles.Admin);
        result.EnsureSucceeded();
        
    }

    public async Task RemoveAsync(Guid targetUserId, Guid currentUserId)
    {
        if (targetUserId == currentUserId)
        {
            throw new ConflictException("admin.cannot_remove_itself", "User cannot remove itself the admin role. Another admin user must do it");
        }

        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("user", targetUserId);
        }

        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        if (!isAdmin)
        {
            throw new NotFoundException("user", user.Id);
        }
        var result = await userManager.RemoveFromRoleAsync(user, AppRoles.Admin);
        result.EnsureSucceeded();

        (await userManager.UpdateSecurityStampAsync(user)).EnsureSucceeded();
    }
}