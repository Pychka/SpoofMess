using AdditionalHelpers;
using CommonObjects.Requests;
using CommonObjects.Results;
using SpoofSettingsService.Models;
using SpoofSettingsService.Repositories;
using SpoofSettingsService.Services;
using SpoofSettingsService.Validators;

namespace SpoofSettingsService.ServiceRealizations;

public class ChatUserService(ILoggerService loggerService, IChatService chatService, IRoleTypeService roleTypeService, UserRepository userRepository, ChatUserRepository chatUserRepository) : IChatUserService
{
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IChatService _chatService = chatService;
    private readonly IRoleTypeService _roleTypeService = roleTypeService;
    private readonly UserRepository _userRepository = userRepository;
    private readonly ChatUserRepository _chatUserRepository = chatUserRepository;
    public async Task<Result> AddMember(AddMemberRequest request, Guid userId)
    {
        try
        {
            UserChatResult result = await _chatService.GetUserAndChat(userId, request.ChatId);
            if (!result.Result.Success)
                return result.Result;

            User? member = await _userRepository.GetByIdAsync(request.MemberId);
            Result validateResult = BaseValidator.ValidateExist(member);
            if (!validateResult.Success) return validateResult;

            RoleType? roleType = await _roleTypeService.GetRoleById(request.RoleId); 
            validateResult = BaseValidator.ValidateExist(roleType);
            if (!validateResult.Success) return validateResult;

            ChatUser newMember = new()
            {
                ChatId = result.User!.Id,
                UserId = member!.Id,
                RoleTypeId = roleType!.Id,
                JoinedAt = DateTime.UtcNow,
            };

            await _chatUserRepository.AddAsync(newMember);
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }

    public Task<Result> ChangeMemberRights()
    {
        throw new NotImplementedException();
    }

    public async Task<Result> RemoveMember(DeleteMemberRequest request, Guid userId)
    {
        try
        {
            UserChatResult result = await _chatService.GetUserAndChat(userId, request.ChatId);
            if (!result.Result.Success)
                return result.Result;

            await _chatUserRepository.DeleteMemberById(request.MemberId, userId);
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }
}
