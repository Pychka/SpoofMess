using CommonObjects.DTO;

namespace CommonObjects.Requests.Avatars;

public record SetChatAvatarRequest(
        FileMetadata Metadata,
        Guid ChatId
    );