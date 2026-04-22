using AdditionalHelpers.Services;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations;

public class ChatAvatarService(
    IChatRepository chatRepository,
    ILoggerService loggerService
    ) : IChatAvatarService
{
    private readonly IChatRepository _chatRepository = chatRepository;
    private readonly ILoggerService _loggerService = loggerService;

    public async Task<Result> Create(CreateChatAvatar createChatAvatar)
    {
        try
        {
            return await _chatRepository.ExecuteUpdateAvatar(
                createChatAvatar.ChatId,
                createChatAvatar.FileId,
                createChatAvatar.OriginalFileName)
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