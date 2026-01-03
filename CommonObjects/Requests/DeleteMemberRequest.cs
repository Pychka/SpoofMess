namespace CommonObjects.Requests;

public class DeleteMemberRequest
{
    public Guid MemberId { get; set; }

    public Guid ChatId { get; set; }
}
