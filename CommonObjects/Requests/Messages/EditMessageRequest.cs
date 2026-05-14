using CommonObjects.Responses;

namespace CommonObjects.Requests.Messages;

public class EditMessageRequest
{
    public Guid Id { get; set; }
    public string? Text { get; set; }
    public List<EditAttachment>? Attachments { get; set; }
    public Guid ChatId { get; set; }
}
