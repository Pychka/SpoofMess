using AdditionalHelpers.Services;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SecurityLibrary;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations;

public class ChatAvatarService(
    IChatRepository chatRepository,
    ILoggerService loggerService,
    IChatEventService chatEventService
    ) : IChatAvatarService
{
    private readonly IChatRepository _chatRepository = chatRepository;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IChatEventService _chatEventService = chatEventService;

    public async Task<Result> Create(CreateChatAvatar createChatAvatar)
    {
        try
        {
            Result result = await _chatRepository.ExecuteUpdateAvatar(
                createChatAvatar.ChatId,
                createChatAvatar.FileId,
                createChatAvatar.OriginalFileName)
                    ? Result.OkResult()
                    : Result.BadRequest("Invalid id");
            _chatEventService.NotifyUpdate(new(createChatAvatar.ChatId, createChatAvatar.UniqueName, createChatAvatar.AccessToken, new([], Hasher.GetKey(createChatAvatar.FileId.ToByteArray()), createChatAvatar.OriginalFileName, 0)));
            return result;
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }
}