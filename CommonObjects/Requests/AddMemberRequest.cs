namespace CommonObjects.Requests;

public class AddMemberRequest
{
    public Guid MemberId { get; set; }

    public Guid ChatId { get; set; }

    public long RoleId { get; set; }
}
