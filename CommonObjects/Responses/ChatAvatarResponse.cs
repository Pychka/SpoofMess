using CommonObjects.DTO;

namespace CommonObjects.Responses;

public class AvatarResponse
{
    public required byte[] AvatarTokenAccess { get; init; }

    public required FileMetadata FileMetadata { get; set; }
}

public record ChatAvatarResponse(Guid ChatId, string UniqueName, byte[] AvatarTokenAccess, FileMetadata FileMetadata);