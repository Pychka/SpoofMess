using CommonObjects.Results;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Validators;

public class ChatValidator
{
    public Result ValidateChat(Chat? chat)
    {
        if (chat is null)
            return Result.NotFoundResult("Chat not exist");

        if(chat.IsDeleted)
            return Result.BadRequest("Chat is deleted");

        return Result.OkResult();
    }
    public Result ValidateChatType(ChatType? chatType)
    {
        if (chatType is null)
            return Result.NotFoundResult("Chat type is not exist");

        return Result.OkResult();
    }

    public Result ValidateHasChatUniqueName(Chat? chat)
    {
        if(chat is not null && !chat.IsDeleted)
            return Result.BadRequest("Unique name of chat has exist");

        return Result.OkResult();
    }

    public Result ValidateChatAndOwner(Chat? chat, Guid userId)
    {
        if (chat is null)
            return Result.NotFoundResult("Chat not exist");

        if (chat.IsDeleted)
            return Result.BadRequest("Chat is deleted");

        if (chat.OwnerId == userId)
            return Result.OkResult();
        else
            return Result.ErrorResult("You is not owner", 403);
    }
}
