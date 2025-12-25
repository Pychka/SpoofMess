using CommonObjects.Requests;
using CommonObjects.Results;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services;
using SpoofSettingsService.Validators;

namespace SpoofSettingsService.ServiceRealizations;

public class UserService(IRepository repository, SpoofSettingsServiceContext context) : IUserService
{
    private readonly IRepository _repository = repository;
    private readonly SpoofSettingsServiceContext _context = context;
    private const string KEY = "user:";
    private static readonly TimeSpan UserExpiration = TimeSpan.FromMinutes(10);

    public async ValueTask<Result> ChangeSettings(ChangeUserSettingsRequest request, long userId)
    {
        User? user = await _repository.GetAsync(KEY + userId,
            async () => await _context.Users.FirstOrDefaultAsync(x => x.Id == userId),
            UserExpiration);
        Result result = UserValidator.Validate(user);

        if (!result.Success) return result;

        user!.Name = request.Name ?? user.Name;
        user.MonthsBeforeDelete = request.MonthsBeforeDelete ?? user.MonthsBeforeDelete;
        user.SearchMe = request.SearchMe ?? user.SearchMe;
        user.ShowMe = request.ShowMe ?? user.ShowMe;
        user.InviteMe = request.InviteMe ?? user.InviteMe;
        user.ForwardMessage = request.ForwardMessage ?? user.ForwardMessage;

        return Result.SuccessResult();
    }

    public async ValueTask<Result> Delete(long userId)
    {
        User? user = await _repository.GetAsync(KEY + userId,
            async () => await _context.Users.FirstOrDefaultAsync(x => x.Id == userId),
            UserExpiration);
        Result result = UserValidator.Validate(user);

        if (!result.Success) return result;

        user!.IsDeleted = true;
        await _repository.ChangeState(_context, KEY + userId, user, EntityState.Deleted, UserExpiration);

        return Result.SuccessResult();
    }
}