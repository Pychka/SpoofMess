using AdditionalHelpers.Services;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SecurityLibrary;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations;

public class UserAvatarService(
    IUserRepository userRepository,
    ILoggerService loggerService,
    IUserEventService userEventService
    ) : IUserAvatarService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IUserEventService _userEventService = userEventService;

    public async Task<Result> Create(CreateUserAvatar createUserAvatar)
    {
        try
        {
            Result result = await _userRepository.ExecuteUpdateAvatar(
                createUserAvatar.UserId,
                createUserAvatar.FileId,
                createUserAvatar.OriginalFileName)
                    ? Result.OkResult()
                    : Result.BadRequest("Invalid id");

            _userEventService.NotifyUpdate(new(null, createUserAvatar.Login, Hasher.GetKey(createUserAvatar.FileId.ToByteArray()), createUserAvatar.AccessToken, createUserAvatar.OriginalFileName, createUserAvatar.CreateTime), createUserAvatar.UserId);
            return result;
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }
}