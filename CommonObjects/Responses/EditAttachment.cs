namespace CommonObjects.Responses;

public record EditAttachment(
    bool IsAdded,
    byte[] Id,
    byte[] Token,
    string OriginalFileName,
    string Category,
    string? Metadata,
    long Size);