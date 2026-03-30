namespace CommonObjects.DTO;

public record FileMetadata(
        byte[] Token,
        byte[] Id,
        string OriginalName,
        long Size);