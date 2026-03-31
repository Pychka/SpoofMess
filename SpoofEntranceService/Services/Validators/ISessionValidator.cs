using CommonObjects.Results;
using SpoofEntranceService.Models;

namespace SpoofEntranceService.Services.Validators;

public interface ISessionValidator
{
    public Result IsInvalidSession(
        SessionInfo? sessionInfo);

    public bool IsSessionTooNew(
        SessionInfo sessionInfo,
        DateTime now);

    public Result ValidateTrustSession(
        SessionInfo? sessionInfo);
}
