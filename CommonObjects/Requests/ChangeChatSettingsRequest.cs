namespace CommonObjects.Requests;

public class ChangeChatSettingsRequest
{
    public Guid Id { get; set; }

    public long? ChatTypeId { get; set; }

    public long? OwnerId { get; set; }

    public string? ChatName { get; set; }

    public bool? IsPublic { get; set; }

    public string? UniqueName { get; set; }
}
