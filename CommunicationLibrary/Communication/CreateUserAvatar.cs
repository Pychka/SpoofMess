namespace CommunicationLibrary.Communication;

public record CreateUserAvatar(
    string Login,
    byte[] AccessToken,
    Guid FileId,
    Guid UserId,
    string OriginalFileName,
    DateTime CreateTime
    ); 
