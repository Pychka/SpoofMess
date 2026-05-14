namespace CommonObjects.DTO;

public record UpdateUserInfo(
    string? Name,
    string Login,
    byte[]? FileId,
    byte[]? AccessToken,
    string? OriginalFileName,
    DateTime Updated
    );