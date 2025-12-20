namespace CommonObjects.Requests;

public class EndSessionRequest
{
    public Guid SessionId { get; set; }
    public string Token { get; set; } = null!;
}
