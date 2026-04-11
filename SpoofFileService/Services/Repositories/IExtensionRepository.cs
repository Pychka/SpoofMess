using SpoofFileService.Models;

namespace SpoofFileService.Services.Repositories;

public interface IExtensionRepository
{
    public Task<ExtensionDto?> GetByName(short id, string extensionName, string category);
}
