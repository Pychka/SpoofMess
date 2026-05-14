namespace CommunicationLibrary.Communication;

public record UpdateUser(
    Guid UserId,
    string Name,
    string Login,
    DateTime Updated
    );