using CommonObjects.Requests;
using CommonObjects.Results;

namespace SpoofSettingsService.Services;

public interface IChatService
{
    public ValueTask<Result> ChangeSettings(ChangeChatSettingsRequest request, long userId);

    public ValueTask<Result> CreateChat(CreateChatRequest request, long userId);

    public ValueTask<Result> DeleteChat(long chatId, long userId);
}
