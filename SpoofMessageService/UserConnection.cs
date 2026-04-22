namespace SpoofMessageService;

public class UserConnection(string ip, Guid sessionId)
{
    public string Ip { get; set; } = ip;
    public Guid SessionId { get; set; } = sessionId;
}