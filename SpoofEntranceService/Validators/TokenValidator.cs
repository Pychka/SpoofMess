using CommonObjects.Results;
using SpoofEntranceService.Models;

namespace SpoofEntranceService.Validators;

public class TokenValidator
{
    public Result ValidateToken(Token? token)
    {
        if (token is null)
            return Result.NotFoundResult($"Token not found");

        if (token.IsDeleted)
            return Result.ErrorResult("Token has been revoked", 401);

        if (token.ValidTo < DateTime.UtcNow)
            return Result.BadRequest("Refresh token expired");    

        return Result.SuccessResult();
    }
}
