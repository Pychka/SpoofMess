using AdditionalHelpers.Services;
using CommonObjects.Requests.Changes;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations;

public class ChatService(
    IChatRepository chatRepository,
    ILoggerService loggerService,
    IChatUserService chatUserService,
    IChatEventService chatEventService
    ) : IChatService
{
    private readonly IChatEventService _chatEventService = chatEventService;
    private readonly IChatUserService _chatUserService = chatUserService;
    private readonly IChatRepository _chatRepository = chatRepository;
    private readonly ILoggerService _loggerService = loggerService;

    public async Task<Result> Create(CreateChat createChat)
    {
        try
        {
            Chat chat = new()
            {
                Name = createChat.Name,
                UniqueName = createChat.UniqueName,
                Id = createChat.Id
            };
            await _chatRepository.AddAsync(chat);

            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }


    public Task<Result> Delete()
    {
        throw new NotImplementedException();
    }


    public async Task<Result> Update(ChangeChatSettingsRequest request, Guid userId)
    {
        try
        {
            Result<ChatUser> result = await _chatUserService.GetAndCheckPermission(request.Id, userId, Models.Enums.Rules.ChangeSettings);
            if (!result.Success)
                return Result.From(result);
            result.Body!.Chat.Name = request.ChatName ?? result.Body!.Chat.Name;
            result.Body!.Chat.IsPublic = request.IsPublic ?? result.Body!.Chat.IsPublic;
            result.Body!.Chat.UniqueName = request.UniqueName ?? result.Body!.Chat.UniqueName;
            await _chatRepository.UpdateAsync(result.Body.Chat);
            _chatEventService.NotifyUpdate(request, result.Body.Chat);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }
}