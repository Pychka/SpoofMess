using CommonObjects.Requests;
using CommonObjects.Results;

namespace SpoofSettingsService.Services;

public interface IChatService
{
    public ValueTask<Result> ChangeSettings(ChangeChatSettingsRequest request, Guid userId);

    public ValueTask<Result> CreateChat(CreateChatRequest request, Guid userId);

    public ValueTask<Result> DeleteChat(Guid chatId, Guid userId);

    public Task<UserChatResult> GetUserAndChat(Guid userId, Guid chatId);
}
