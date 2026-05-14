namespace CommonObjects.DTO;

public record SearchableMessage(Guid ChatId, Guid Id, string? Text, DateTime SentAt);