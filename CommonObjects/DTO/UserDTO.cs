namespace CommonObjects.DTO;

public record UserDTO(
        string Name,
        string Login,
        byte[]? AvatarToken
    );
