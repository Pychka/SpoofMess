namespace CommunicationLibrary.Communication;

public record CreateFile(
        Guid FileId,
        long Size,
        string Category,
        string? Metadata
    );