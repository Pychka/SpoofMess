using CommonObjects.Responses;
using SpoofEntranceService.Models;

namespace SpoofEntranceService;
internal readonly record struct TokenResponse(
    Token Token,
    UserAuthorizeResponse Response
    );