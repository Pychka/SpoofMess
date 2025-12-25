using CommonObjects.Requests;
using CommonObjects.Results;

namespace SpoofSettingsService.Services;

public interface IUserService
{
    public ValueTask<Result> ChangeSettings(ChangeUserSettingsRequest request, long userId);

    public ValueTask<Result> Delete(long userId);
}
