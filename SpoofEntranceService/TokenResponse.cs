using CommonObjects.Responses;
using SpoofEntranceService.Models;

namespace SpoofEntranceService;
public readonly record struct TokenResponse(
    Token Token,
    UserAuthorizeResponse Response
    );