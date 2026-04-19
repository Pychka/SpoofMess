using AdditionalHelpers.Services;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SpoofMessageService.Models;
using SpoofMessageService.Models.Enums;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Repositories;
using SpoofMessageService.Services.Validators;

namespace SpoofMessageService.ServiceRealizations;

public class ChatUserService(
    IChatUserRepository chatUserRepository,
    IChatUserValidator chatUserValidator,
    ILoggerService loggerService,
    IRuleParserService ruleParserService) : IChatUserService
{
    private readonly IChatUserRepository _chatUserRepository = chatUserRepository;
    private readonly IChatUserValidator _chatUserValidator = chatUserValidator;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IRuleParserService _ruleParserService = ruleParserService;

    public async Task<Result> Add(CreateChatUser createChatUser)
    {
        try
        {
            ChatUser chatUser = new()
            {
                Key1 = createChatUser.ChatId,
                Key2 = createChatUser.UserId,
                JoinedAt = DateTime.UtcNow,
                Rules = _ruleParserService.ParseRules(createChatUser.Rules)
            };
            await _chatUserRepository.AddAsync(chatUser);

            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error($"An error occurred while saving chat users: {ex.Message}");
            return Result.ErrorResult("An error occurred while saving chat users.");
        }
    }

    public async Task<Result> Delete(Guid chatId, Guid userId)
    {
        try
        {
            await _chatUserRepository.Delete(chatId, userId);

            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error($"An error occurred while getting chat users: {ex.Message}");
            return Result.ErrorResult("An error occurred while getting chat users.");
        }
    }

    public async Task<Result<ChatUser>> GetAndCheckPermission(Guid chatId, Guid userId, Rules rule)
    {
        Result<ChatUser> chatUserResult = await GetMember(chatId, userId);
        if (chatUserResult.Success)
        {
            Result result = _chatUserValidator.HasPermission(chatUserResult.Body!, rule);
            if (result.Success)
                return chatUserResult;
            return Result<ChatUser>.From(result);
        }
        return chatUserResult;
    }

    public async Task<Result> Update(UpdateChatUser updateChatUser)
    {
        try
        {
            ChatUser? chatUser = await _chatUserRepository.GetByIdAsync(updateChatUser.ChatId, updateChatUser.UserId);
            Result result = _chatUserValidator.IsAvailable(chatUser);
            if (!result.Success)
                return result;

            chatUser!.Rules = _ruleParserService.ParseRules(updateChatUser.Rules);
            await _chatUserRepository.UpdateAsync(chatUser);

            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error($"An error occurred while saving chat users: {ex.Message}");
            return Result.ErrorResult("An error occurred while saving chat users.");
        }
    }

    public async Task<Result<ChatUser>> GetMember(Guid chatId, Guid userId)
    {
        try
        {
            ChatUser? chatUser = await _chatUserRepository.GetByIdAsync(chatId, userId);
            Result result = _chatUserValidator.IsAvailable(chatUser);
            if (!result.Success)
                return Result<ChatUser>.From(result);

            return Result<ChatUser>.OkResult(chatUser!);
        }
        catch (Exception ex)
        {
            _loggerService.Error($"An error occurred while getting chat users: {ex.Message}");
            return Result<ChatUser>.ErrorResult("An error occurred while getting chat users.");
        }
    }

    public async Task<Result<List<ChatUser>>> GetMembers(Guid chatId)
    {
        try
        {
            List<ChatUser> chatUsers = await _chatUserRepository.GetManyByChatId(chatId);
            Result result = _chatUserValidator.IsAvailableCollection(chatUsers);
            if (!result.Success)
                return Result<List<ChatUser>>.From(result);

            return Result<List<ChatUser>>.OkResult(chatUsers!);
        }
        catch (Exception ex)
        {
            _loggerService.Error($"An error occurred while getting chat users: {ex.Message}");
            return Result<List<ChatUser>>.ErrorResult("An error occurred while getting chat users.");
        }
    }

    public async Task<Result<List<ChatUser>>> GetChats(Guid userId)
    {
        try
        {
            List<ChatUser> chatUsers = await _chatUserRepository.GetManyByUserId(userId);
            Result result = _chatUserValidator.IsAvailableCollection(chatUsers);
            if (!result.Success)
                return Result<List<ChatUser>>.From(result);

            return Result<List<ChatUser>>.OkResult(chatUsers!);
        }
        catch (Exception ex)
        {
            _loggerService.Error($"An error occurred while getting chat users: {ex.Message}");
            return Result<List<ChatUser>>.ErrorResult("An error occurred while getting chat users.");
        }
    }
}
