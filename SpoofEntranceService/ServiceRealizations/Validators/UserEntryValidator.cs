using CommonObjects.Results;
using DataSaveHelpers.ServiceRealizations;
using SpoofEntranceService.Models;
using SpoofEntranceService.Services.Validators;

namespace SpoofEntranceService.ServiceRealizations.Validators;

public class UserEntryValidator : SoftDeletableValidator<UserEntry>, IUserEntryValidator
{
    public Result HisIsActive(UserEntry? user)
    {
        if (user is null || user.IsDeleted)
            return Result.OkResult();

        return Result.BadRequest("Login is busy");
    }
}