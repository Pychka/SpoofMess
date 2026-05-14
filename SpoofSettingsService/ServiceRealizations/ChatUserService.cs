using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Requests.ChatUsers;
using CommonObjects.Requests.Members;
using CommonObjects.Results;
using RuleRoleHelper;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services;
using SpoofSettingsService.Services.MessageBrokers;
using SpoofSettingsService.Services.Repositories;
using SpoofSettingsService.Services.Validators;

namespace SpoofSettingsService.ServiceRealizations;

public class ChatUserService(
    ILoggerService loggerService,
    IChatUserValidator chatUserValidator,
    IRuleService ruleService,
    IChatService chatService,
    IUserService userService,
    IChatUserRepository chatUserRepository,
    IChatUserPublisherService chatUserPublisherService,
    IFileTokenService fileTokenService
    ) : IChatUserService
{
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly IUserService _userService = userService;
    private readonly IChatUserValidator _chatUserValidator = chatUserValidator;
    private readonly IChatUserRepository _chatUserRepository = chatUserRepository;
    private readonly IRuleService _ruleService = ruleService;
    private readonly IChatService _chatService = chatService;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IChatUserPublisherService _chatUserPublisherService = chatUserPublisherService;

    public async Task<Result> Join(JoinToChatRequest request, Guid userId)
    {
        try
        { 
            Task<Result<Chat>> chatResult = _chatService.Get(request.ChatId);
            Task<Result<User>> memberResult = _userService.Get(userId);

            await Task.WhenAll(chatResult, memberResult);

            if (!chatResult.Result.Success)
                return Result.From(chatResult.Result);

            if (!memberResult.Result.Success)
                return Result.From(memberResult.Result);

            ChatUser newMember = new()
            {
                Key1 = chatResult.Result.Body!.Id,
                Key2 = memberResult.Result.Body!.Id,
                JoinedAt = DateTime.UtcNow,
            };
            await _chatUserRepository.AddAsync(newMember);
            _ = Task.Run(async () =>
            {
                Result<CommunicationLibrary.Communication.Rule[]> rulesResult = await _ruleService.ChatUserRulesForSMS(newMember.Key1, newMember.Key2);
                if (rulesResult.Success)
                    await _chatUserPublisherService.Create(newMember, rulesResult.Body!);
                else
                    _loggerService.Error($"Can't find rules for: {newMember.Key1}, {newMember.Key2}\n{rulesResult.Error}");
            });
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }

    public async Task<Result> Add(AddMemberRequest request, Guid userId)
    {
        try
        {
            Result result = await _ruleService.HasPermissionAsync(
                    userId,
                    request.ChatId,
                    Permissions.Inviting
                );
            if (!result.Success)
                return result;

            Result<User> memberResult = await _userService.Get(request.MemberId);

            if (!memberResult.Success) return Result.From(memberResult);

            ChatUser newMember = new()
            {
                Key1 = request.ChatId,
                Key2 = memberResult.Body!.Id,
                JoinedAt = DateTime.UtcNow,
            };
            await _chatUserRepository.AddAsync(newMember);
            _ = Task.Run(async () =>
            {
                Result<CommunicationLibrary.Communication.Rule[]> rulesResult = await _ruleService.ChatUserRulesForSMS(newMember.Key1, newMember.Key2);
                if (rulesResult.Success)
                    await _chatUserPublisherService.Create(newMember, rulesResult.Body!);
                else
                    _loggerService.Error($"Can't find rules for: {newMember.Key1}, {newMember.Key2}\n{rulesResult.Error}");
            });
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }

    public async Task<Result<ChatUser>> Get(GetChatUserRequest request, Guid userId)
    {
        try
        {
            Task<ChatUser?> taskRequester = _chatUserRepository.GetByIdAsync(
                    request.ChatId,
                    userId
                );
            Task<ChatUser?> taskChatUser = _chatUserRepository.GetWithRules(
                    request.ChatId,
                    request.UserId
                );
            await Task.WhenAll(
                    taskRequester,
                    taskChatUser
                );

            Result result = _chatUserValidator.IsAvailable(taskRequester.Result);
            if (!result.Success) return Result<ChatUser>.From(result);

            result = _chatUserValidator.IsAvailable(taskChatUser.Result);
            if (!result.Success) return Result<ChatUser>.From(result);

            return Result<ChatUser>.OkResult(taskChatUser.Result!);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<ChatUser>.ErrorResult("DataBase error");
        }
    }

    public async Task<Result<List<ChatUserDTO>>> GetUserChats(Guid userId, DateTime after)
    {
        try
        {
            List<ChatUserDTO> chats = await _chatUserRepository.GetUserChatsAfterDate(userId, after);
            if(chats is null or [])
                return Result<List<ChatUserDTO>>.BadRequest("Invalid token");

            foreach(ChatUserDTO chatUser in chats)
            {
                if (chatUser.FileId is null)
                    continue;
                chatUser.AvatarFileToken = _fileTokenService.CreateToken(userId, chatUser.FileId.Value);
                chatUser.AvatarId = Hasher.GetKey(chatUser.FileId.Value.ToByteArray());
            }
            return Result<List<ChatUserDTO>>.OkResult(chats);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<List<ChatUserDTO>>.ErrorResult("DataBase error");
        }
    }


    public async Task<Result> Remove(DeleteMemberRequest request, Guid userId)
    {
        try
        {
            Task<Result> taskRuleResult = _ruleService.HasPermissionAsync(
                    userId,
                    request.ChatId,
                    Permissions.Kicking
                );
            Task<Result<Chat>> taskChat = _chatService.Get(userId);

            await Task.WhenAll(
                    taskRuleResult,
                    taskChat
                );

            if (!taskRuleResult.Result.Success)
                return taskRuleResult.Result;

            if (!taskChat.Result.Success)
                return Result.From(taskChat.Result);

            await _chatUserRepository.SoftDeleteAsync(request.ChatId, request.ChatId);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }
}
