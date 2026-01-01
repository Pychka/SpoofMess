using CommonObjects.Results;
using SpoofEntranceService.Models;

namespace SpoofEntranceService.Validators;

public class SessionValidator
{
    public Result IsInvalidSession(SessionInfo? sessionInfo)
    {
        if (sessionInfo is null)
            return Result.NotFoundResult($"Not found your session");

        if (sessionInfo.IsDeleted || !sessionInfo.IsActive)
            return Result.BadRequest("Session is disabled or is deleted");

        return Result.SuccessResult();
    }

    public bool IsSessionTooNew(SessionInfo sessionInfo, DateTime now) =>
        sessionInfo.CreatedAt.Date >= now.AddDays(-7).Date;


    public Result ValidateTrustSession(SessionInfo? sessionInfo)
    {
        Result result = IsInvalidSession(sessionInfo);
        if(result.Success)
            result = IsSessionTooNew(sessionInfo!, DateTime.UtcNow) ? Result.ErrorResult("No trust", 403) : Result.SuccessResult();

        return result;
    }
}