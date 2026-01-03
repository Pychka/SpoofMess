using CommonObjects.Requests;
using CommonObjects.Results;
using DataHelpers.ServiceRealizations;
using SpoofSettingsService.Models;
using SpoofSettingsService.Repositories;
using SpoofSettingsService.Services;
using SpoofSettingsService.Setter;
using SpoofSettingsService.Validators;

namespace SpoofSettingsService.ServiceRealizations;

public class ChatService(ChatRepository chatRepository, UserRepository userRepository, Repository<ChatType, long> chatTypeRepository, ChatValidator chatValidator) : IChatService
{
    private readonly ChatValidator _chatValidator = chatValidator;
    private readonly ChatRepository _chatRepository = chatRepository;
    private readonly UserRepository _userRepository = userRepository;
    private readonly Repository<ChatType, long> _chatTypeRepository = chatTypeRepository;

    public async ValueTask<Result> ChangeSettings(ChangeChatSettingsRequest request, Guid userId)
    {
        UserChatResult result = await GetUserAndChat(userId, request.Id);
        if (!result.Result.Success)
            return result.Result;

        result.Chat!.Set(request);
        return Result.OkResult();
    }


    public async ValueTask<Result> CreateChat(CreateChatRequest request, Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        Result result = UserValidator.Validate(user);
        if (!result.Success) return result;

        ChatType? chatType = await _chatTypeRepository.GetByIdAsync(request.ChatTypeId);
        result = _chatValidator.ValidateChatType(chatType);
        if (!result.Success) return result;

        Chat? repetition = await _chatRepository.GetByUniqueName(request.UniqueName);
        result = _chatValidator.ValidateHasChatUniqueName(repetition);
        if (!result.Success) return result;

        DateTime now = DateTime.UtcNow;
        Chat newChat = new(request.ChatTypeId, user!.Id, request.ChatName, request.IsPublic, request.UniqueName, now, now);

        await _chatRepository.Change(newChat, repetition);

        return Result.OkResult();
    }

    public async ValueTask<Result> DeleteChat(Guid chatId, Guid userId)
    {
        UserChatResult result = await GetUserAndChat(userId, chatId);
        if (!result.Result.Success)
            return result.Result;

        await _chatRepository.SoftDeleteAsync(result.Chat!);
        return Result.OkResult();
    }

    public async Task<UserChatResult> GetUserAndChat(Guid userId, Guid chatId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        Result result = UserValidator.Validate(user);
        if (!result.Success) return new(null, null, result);

        Chat? chat = await _chatRepository.GetByIdAsync(chatId);

        result = _chatValidator.ValidateChatAndOwner(chat, userId);
        if (!result.Success) return new(null, null, result);

        return new(user, chat, Result.OkResult());
    }
}