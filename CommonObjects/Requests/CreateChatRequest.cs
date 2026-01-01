namespace CommonObjects.Requests;

public class CreateChatRequest
{
    public long ChatTypeId { get; set; }

    public required string ChatName { get; set; }

    public bool IsPublic { get; set; }

    public required string UniqueName { get; set; }
}
