using CommonObjects.Results;
using DataSaveHelpers.Services;
using SpoofEntranceService.Models;

namespace SpoofEntranceService.Services.Validators;

public interface IUserEntryValidator : ISoftDeletableValidator<UserEntry>
{
    public Result HisIsActive(UserEntry? user);
}
