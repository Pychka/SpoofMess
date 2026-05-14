namespace CommonObjects.DTO;

public record UserDTO(
        string Name,
        string Login,
        byte[]? AvatarId,
        byte[]? AvatarToken,
        string? AvatarOriginalFileName
    );
