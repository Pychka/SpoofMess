using AdditionalHelpers.Services;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;
using SpoofMessageService.Services.Repositories;
using SpoofMessageService.Services.Validators;

namespace SpoofMessageService.ServiceRealizations;

public class UserService(
    IUserRepository userRepository,
    ILoggerService loggerService,
    IUserValidator userValidator,
    IUserEventsService userEventsService
    ) : IUserService
{
    private readonly IUserEventsService _userEventsService = userEventsService;
    private readonly IUserValidator _userValidator = userValidator;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILoggerService _loggerService = loggerService;

    public async Task<Result> Create(CreateUser createUser)
    {
        try
        {
            User user = new()
            {
                Name = createUser.Name,
                Login = createUser.Login,
                Id = createUser.UserId
            };
            await _userRepository.AddAsync(user);

            return Result.OkResult();
        }
        catch(Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }

    public async Task<Result> Delete(
            Guid userId
        )
    {
        try
        {
            return await _userRepository.DeleteById(userId)
                ? Result.OkResult()
                : Result.BadRequest("Invalid id");
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }

    public async Task<Result> Update(UpdateUser updateUser)
    {
        try
        {
            User? user = await _userRepository.GetByIdAsync(updateUser.UserId);
            Result result = _userValidator.IsAvailable(user);
            if (!result.Success) return result;
            user!.Name = updateUser.Name;
            user.Login = updateUser.Login;
            user.LastModified = updateUser.Updated;
            await _userRepository.UpdateAsync(user);
            _userEventsService.NotifyUpdate(new(
                updateUser.Name,
                user.Login,
                null,
                null,
                DateTime.UtcNow),
                updateUser.UserId);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }

    public async Task<Result> ChangeConnectionState(Guid userId, bool state)
    {
        try
        { 
            return await _userRepository.ExecuteUpdateConnection(userId, state)
                ? Result.OkResult()
                : Result.BadRequest("Invalid id");
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }
}
