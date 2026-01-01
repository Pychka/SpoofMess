using CommonObjects.Results;
using SpoofEntranceService.Models;

namespace SpoofEntranceService.Validators;

public class UserEntryValidator
{
    public Result IsActive(UserEntry? user)
    {
        if (user is null)
            return Result.NotFoundResult("User is not exist");
        if (user.IsDeleted)
            return Result.BadRequest($"User {user.Id} has been deleted");

        return Result.SuccessResult();
    }

    public Result HisIsActive(UserEntry? user)
    {
        if (user is null || user.IsDeleted)
            return Result.SuccessResult();

        return Result.BadRequest("Login is busy");
    }

}
