namespace CommunicationLibrary.Communication;

public record CreateChatAvatar(
    string UniqueName,
    byte[] AccessToken,
    Guid FileId,
    Guid ChatId,
    string OriginalFileName,
    DateTime CreateTime
    );