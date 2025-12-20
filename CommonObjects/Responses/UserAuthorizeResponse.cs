using CommonObjects.DTO;

namespace CommonObjects.Responses;

public class UserAuthorizeResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public SessionInfo SessionInfo { get; set; } = null!;
}
