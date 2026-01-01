using CommonObjects.Requests;
using CommonObjects.Results;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;
using SpoofSettingsService.Repositories;
using SpoofSettingsService.Services;
using SpoofSettingsService.Setter;
using SpoofSettingsService.Validators;

namespace SpoofSettingsService.ServiceRealizations;

public class UserService(UserRepository userRepository) : IUserService
{
    private readonly UserRepository _userRepository = userRepository;

    public async ValueTask<Result> ChangeSettings(ChangeUserSettingsRequest request, long userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        Result result = UserValidator.Validate(user);

        if (!result.Success) return result;

        user!.Set(request);
        return Result.SuccessResult();
    }

    public async ValueTask<Result> Delete(long userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        Result result = UserValidator.Validate(user);

        if (!result.Success) return result;

        user!.IsDeleted = true;
        await _userRepository.SoftDeleteAsync(user);

        return Result.SuccessResult();
    }
}