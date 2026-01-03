using CommonObjects.Results;
using DataHelpers;

namespace SpoofSettingsService.Validators;

public class BaseValidator
{
    public static Result ValidateExist<T>(T? entity) where T : ISoftDeletable
    {
        if (entity is null || entity.IsDeleted)
            return Result.BadRequest($"Invalid {typeof(T).Name.ToLower()}");

        return Result.SuccessResult();
    }
}
