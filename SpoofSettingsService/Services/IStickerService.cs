using CommonObjects.Results;

namespace SpoofSettingsService.Services;

public interface IStickerService
{
    public Task<Result> CreateAsync();
}
