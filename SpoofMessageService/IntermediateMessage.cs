namespace SpoofMessageService;

public readonly record struct IntermediateMessage(
        Guid Id,
        Guid ChatId,
        string SenderLogin,
        string SenderName,
        Guid? SenderAvatar,
        string OriginalFileName,
        string Text,
        DateTime SendAt,
        MessageAttachment[] Attachments
    );
public readonly record struct MessageAttachment(
        string OriginalFileName,
        string Category,
        long FileSize,
        string? Metadata,
        Guid Id
    );