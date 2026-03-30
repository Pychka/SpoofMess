namespace CommonObjects.Requests.Attachments;

public record Attachment(
        byte[] Id,
        byte[] Token,
        string OriginalFileName,
        string Category,
        string? Metadata,
        long Size
    );
