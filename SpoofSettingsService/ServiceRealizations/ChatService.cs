using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Requests;
using CommonObjects.Requests.Changes;
using CommonObjects.Results;
using RuleRoleHelper;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services;
using SpoofSettingsService.Services.MessageBrokers;
using SpoofSettingsService.Services.Repositories;
using SpoofSettingsService.Services.Validators;
using SpoofSettingsService.Setters;

namespace SpoofSettingsService.ServiceRealizations;

public class ChatService(
        IChatRepository chatRepository,
        IChatTypeService chatTypeService,
        IChatPublisherService chatPublisherService,
        IChatValidator chatValidator,
        IUserService userService,
        IRuleService ruleService,
        ILoggerService loggerService,
        IFileTokenService fileTokenService
    ) : IChatService
{
    private readonly IChatValidator _chatValidator = chatValidator;
    private readonly IUserService _userService = userService;
    private readonly IChatRepository _chatRepository = chatRepository;
    private readonly IChatTypeService _chatTypeService = chatTypeService;
    private readonly IRuleService _ruleService = ruleService;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly IChatPublisherService _chatPublisherService = chatPublisherService;

    public async ValueTask<Result> ChangeSettings(ChangeChatSettingsRequest request, Guid userId)
    {
        try
        {
            Chat? chat = await _chatRepository.GetByIdAsync(request.Id);
            Result result = _chatValidator.IsAvailable(chat);
            if (!result.Success)
                return result;
            Result permissionResult = await _ruleService.HasPermissionAsync(
                    userId, 
                    request.Id,
                    Permissions.ChangeSettings
                );
            if (!permissionResult.Success)
                return permissionResult;

            chat!.Set(request);
            await _chatRepository.UpdateAsync(chat!);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }


    public async Task<Result<ChatDTO>> CreateChat(CreateChatRequest request, Guid userId)
    {
        try
        {
            Task<Result<User>> taskUserResult = _userService.Get(userId);

            Task<Result<ChatType>> taskChatTypeResult = _chatTypeService.Get(request.ChatTypeId);

            await Task.WhenAll(
                taskUserResult,
                taskChatTypeResult
                );

            if (!taskUserResult.Result.Success)
                return Result<ChatDTO>.From(taskUserResult.Result);

            if (!taskChatTypeResult.Result.Success)
                return Result<ChatDTO>.From(taskChatTypeResult.Result);

            Chat? repetition = await _chatRepository.GetByUniqueName(request.UniqueName);
            Result result = _chatValidator.ValidateHasChatUniqueName(repetition);
            if (!result.Success) return Result<ChatDTO>.From(result);

            DateTime now = DateTime.UtcNow;
            Chat newChat = new(
                Guid.CreateVersion7(),
                request.ChatTypeId,
                taskUserResult.Result.Body!.Id,
                request.ChatName,
                request.UniqueName,
                now,
                now);

            await _chatRepository.Change(newChat, repetition);
            await _chatPublisherService.Publish(
                new(
                    newChat.Id,
                    null,
                    newChat.UniqueName,
                    newChat.Name
                    )
                );
            return Result<ChatDTO>.OkResult(newChat.Set(null, null));
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result<ChatDTO>.ErrorResult("Database error");
        }
    }

    public async ValueTask<Result> DeleteChat(Guid chatId, Guid userId)
    {
        try
        {
            Result<ChatWithOwner> result = await GetChatWithOwner(userId, chatId);
            if (!result.Success)
                return Result.From(result);

            await _chatRepository.SoftDeleteAsync(result.Body.Chat!);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result.ErrorResult("Database error");
        }
    }

    public async Task<Result<Chat>> Get(Guid chatId)
    {
        try
        {
            Chat? chat = await _chatRepository.GetAsync(chatId);
            Result result = _chatValidator.IsAvailable(chat);
            if(!result.Success)
                return Result<Chat>.From(result);

            return Result<Chat>.OkResult(chat!);
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result<Chat>.ErrorResult("Database error", 400);
        }
    }

    public async Task<Result<ChatWithOwner>> GetChatWithOwner(Guid userId, Guid chatId)
    {
        try
        {
            Task<Result<User>> userResult = _userService.Get(userId);
            Task<Chat?> chatResult = _chatRepository.GetByIdAsync(chatId);
            await Task.WhenAll(userResult, chatResult);

            if (!userResult.Result.Success)
                return Result<ChatWithOwner>.From(userResult.Result);

            Result result = _chatValidator.ValidateChatAndOwner(chatResult.Result, userId);
            if (!result.Success)
                return Result<ChatWithOwner>.From(result);

            return Result<ChatWithOwner>.OkResult(
                new(
                    userResult.Result.Body!,
                    chatResult.Result!
                    )
                );
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result<ChatWithOwner>.ErrorResult("Database error");
        }
    }

    public async Task<Result<ChatDTO>> GetChat(Guid userId, Guid chatId)
    {
        try
        {
            Result<Chat> result = await Get(chatId);
            if (!result.Success)
                return Result<ChatDTO>.From(result);

            return Result<ChatDTO>.OkResult(new(
                    result.Body!.Id,
                    result.Body.ChatTypeId,
                    result.Body.UniqueName,
                    result.Body.Name,
                    result.Body.ActualAvatar is null ? null :
                    _fileTokenService.CreateToken(userId, result.Body.ActualAvatar.FileId),
                    result.Body.ActualAvatar is null ? null :
                    Hasher.GetKey(result.Body.ActualAvatar.FileId.ToByteArray()),
                    result.Body.ActualAvatar?.OriginalFileName,
                    result.Body.CreatedAt,
                    result.Body.OwnerId
                ));
        }
        catch (Exception ex)
        {
            _loggerService.Error("Database error", ex);
            return Result<ChatDTO>.ErrorResult("Database error");
        }
    }
}