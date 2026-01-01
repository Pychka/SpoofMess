using CommonObjects.DTO;

namespace CommonObjects.Responses;

public class UserAuthorizeResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public SessionInfo SessionInfo { get; set; } = null!;

    public UserAuthorizeResponse() { }

    public UserAuthorizeResponse(string accessToken, string refreshToken, SessionInfo sessionInfo)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        SessionInfo = sessionInfo;
    }
}
