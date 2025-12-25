using CommonObjects.Results;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Validators;

public static class UserValidator
{
    public static Result Validate(User? user)
    {
        if (user is null)
            return Result.ErrorResult("Invalid id", 400);

        if (user.IsDeleted)
            return Result.ErrorResult("User was deleted", 400);

        return Result.SuccessResult();
    }
}
