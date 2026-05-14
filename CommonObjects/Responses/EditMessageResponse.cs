namespace CommonObjects.Responses;

public record EditMessageResponse(
        Guid Id,
        Guid ChatId,
        string SenderLogin,
        string SenderName,
        string? Text,
        DateTime LastModified,
        List<EditAttachment> Attachments
    );
