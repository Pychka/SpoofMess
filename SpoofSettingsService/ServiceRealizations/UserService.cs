using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Requests.Changes;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services;
using SpoofSettingsService.Services.MessageBrokers;
using SpoofSettingsService.Services.Repositories;
using SpoofSettingsService.Services.Validators;
using SpoofSettingsService.Setters;

namespace SpoofSettingsService.ServiceRealizations;

public class UserService(
    ILoggerService logger,
    IUserRepository userRepository,
    IUserValidator userValidator,
    IUserMessageBrokerService userMessageBrokerService,
    IFileTokenService fileTokenService) : IUserService
{
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly ILoggerService _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserValidator _userValidator = userValidator;
    private readonly IUserMessageBrokerService _userMessageBrokerService = userMessageBrokerService;

    public async Task<Result<User>> Get(Guid id)
    {
        try
        {
            User? user = await _userRepository.GetByIdAsync(id);
            Result result = _userValidator.IsAvailable(user);

            if (!result.Success) return Result<User>.From(result);

            return Result<User>.OkResult(user!);
        }
        catch (Exception ex)
        {
            _logger.Error("Database error", ex);
            return Result<User>.ErrorResult("Database error");
        }
    }
    public async Task<Result<UserDTO>> GetInfo(string login, Guid requesterId)
    {
        try
        {
            User? user = await _userRepository.GetByLogin(login);
            Result result = _userValidator.IsAvailable(user);

            if (!result.Success) 
                return Result<UserDTO>.From(result);

            UserAvatar? avatar = user!.UserAvatars.FirstOrDefault(a => a.IsActive);
            return Result<UserDTO>.OkResult(
                user.Set(avatar is null
                    ? null
                    : _fileTokenService.CreateToken(
                        requesterId,
                        avatar.Id),
                    avatar is null
                    ? null
                    : Hasher.GetKey(avatar.Id.ToByteArray()),
                    avatar?.OriginalFileName)
                );
        }
        catch (Exception ex)
        {
            _logger.Error("Database error", ex);
            return Result<UserDTO>.ErrorResult("Database error");
        }
    }

    public async Task<Result> ChangeSettings(ChangeUserSettingsRequest request, Guid userId)
    {
        try
        {
            User? user = await _userRepository.GetByIdAsync(userId);
            Result result = _userValidator.IsAvailableAndHisOwner(user, userId);

            if (!result.Success) return result;

            user!.Set(request);
            await _userRepository.UpdateAsync(user!);
            await _userMessageBrokerService.ConfirmUpdate(new(user!.Id, user.Name, user.Login, user.LastModified));
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _logger.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }

    public async Task<Result> Delete(Guid userId)
    {
        try
        {
            User? user = await _userRepository.GetByIdAsync(userId);
            Result result = _userValidator.IsAvailableAndHisOwner(user, userId);

            if (!result.Success) return result;
            user!.IsDeleted = true;

            await _userRepository.SoftDeleteAsync(user);
            await _userMessageBrokerService.ConfirmDelete(new(userId, "", ""));

            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _logger.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }

    public async Task<Result> Create(CreateUser createUser)
    {
        try
        {
            User user = new()
            {
                Id = createUser.UserId,
                Name = createUser.Name,
                Login = createUser.Login
            };
            await _userRepository.AddAsync(user);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _logger.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }
}