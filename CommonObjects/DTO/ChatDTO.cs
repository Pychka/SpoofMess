namespace CommonObjects.DTO;

public record ChatDTO(
        Guid Id,
        int ChatTypeId,
        string UniqueName,
        string Name,
        byte[]? AvatarToken,
        byte[]? AvatarId,
        string? OriginalAvatarFileName,
        DateTime CreatedAt,
        Guid? OwnerId
    );
