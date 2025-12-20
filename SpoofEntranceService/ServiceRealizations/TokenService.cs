using CommonObjects.Requests;
using CommonObjects.Responses;
using CommonObjects.Results;
using Microsoft.EntityFrameworkCore;
using SecurityLibrary;
using SpoofEntranceService.Models;
using AdditionalHelpers;
using SpoofEntranceService.Converters;
using DataHelpers.Services;
using SpoofEntranceService.Services;

namespace SpoofEntranceService.ServiceRealizations;

public class TokenService(IRepository repository, SpoofEntranceServiceDbContext context, ILoggerService loger) : ITokenService
{
    private readonly SpoofEntranceServiceDbContext _context = context;
    private readonly IRepository _repository = repository;
    private readonly ILoggerService _logService = loger;

    private const string KEY = "refresh_token:";
    public async Task<Result<UserAuthorizeResponse>> Create(SessionInfo sessionInfo)
    {
        (string Token, string TokenHash) refresh = Tokenizer.CreateRefresh();
        string access = Tokenizer.CreateAccess(sessionInfo.UserEntry.UniqueName, sessionInfo.UserEntryId, sessionInfo.Id);

        Token newToken = new()
        {
            RefreshTokenHash = refresh.TokenHash,
            SessionInfoId = sessionInfo.Id,
            ValidTo = DateTime.UtcNow.AddDays(30)
        };

        try
        {
            newToken = await _repository.ChangeState(
                _context,
                $"{KEY}{refresh.TokenHash}",
                newToken,
                expiration: TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
            return Result<UserAuthorizeResponse>.ErrorResult("Internal server error", 500);
        }

        return Result<UserAuthorizeResponse>.SuccessResult("Ok", new()
        {
            AccessToken = access,
            RefreshToken = refresh.Token,
            SessionInfo = sessionInfo.ToDTO()
        });
    }


    public async Task<Result<UserAuthorizeResponse>> UpdateToken(UpdateTokenRequest tokenRequest)
    {
        try
        {
            string hashToken = Hasher.HashKey(tokenRequest.Token);
            Token? oldToken = await _repository.GetAsync(
                    $"{KEY}{hashToken}",
                    async () => await _context.Tokens.Include(x => x.SessionInfo).ThenInclude(x => x.UserEntry).FirstOrDefaultAsync(x => hashToken == x.RefreshTokenHash)
                );

            if (oldToken is null)
                return Result<UserAuthorizeResponse>.NotFoundResult($"Not found {tokenRequest.Token}");
            if (oldToken.ValidTo < DateTime.UtcNow)
                return Result<UserAuthorizeResponse>.ErrorResult("Refresh token expired", 400);

            (string Token, string TokenHash) refresh = Tokenizer.CreateRefresh();
            string access = Tokenizer.CreateAccess(oldToken.SessionInfo.UserEntry.UniqueName, oldToken.SessionInfo.UserEntryId, oldToken.SessionInfoId);

            Token newToken = new()
            {
                RefreshTokenHash = refresh.TokenHash,
                SessionInfoId = oldToken.SessionInfoId,
                ValidTo = DateTime.UtcNow.AddDays(30)
            };

            await _repository.ChangeStates(
                _context,
                [
                    ($"{KEY}{oldToken.RefreshTokenHash}", oldToken, EntityState.Deleted), 
                    ($"{KEY}{newToken.RefreshTokenHash}", newToken, EntityState.Added)
                ], 
                expiration: TimeSpan.FromDays(30));

            return Result<UserAuthorizeResponse>.SuccessResult("Ok", new()
            {
                AccessToken = access,
                RefreshToken = refresh.Token,
                SessionInfo = oldToken.SessionInfo.ToDTO()
            });
        }
        catch (Exception ex)
        {
            _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
            return Result<UserAuthorizeResponse>.ErrorResult("Internal server error", 500);
        }
    }
}
