using SpoofSettingsService.Models;

namespace SpoofSettingsService.Services;

public interface IRoleTypeService
{
    public Task<RoleType?> GetRoleById(long roleId);
}
