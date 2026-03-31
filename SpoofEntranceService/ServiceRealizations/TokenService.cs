using AdditionalHelpers.Services;
using CommonObjects.Requests;
using CommonObjects.Responses;
using CommonObjects.Results;
using SecurityLibrary;
using SpoofEntranceService.Converters;
using SpoofEntranceService.Models;
using SpoofEntranceService.Services;
using SpoofEntranceService.Services.Repositories;
using SpoofEntranceService.Services.Validators;

namespace SpoofEntranceService.ServiceRealizations;

public class TokenService(
    ITokenRepository repository, 
    ITokenValidator tokenValidator, 
    ILoggerService loger) : ITokenService
{
    private readonly ITokenRepository _repository = repository;
    private readonly ILoggerService _logService = loger;
    private readonly ITokenValidator _tokenValidator = tokenValidator;

    public async Task<Result<TokenResponse>> Create(SessionInfo sessionInfo)
    {
        return Result<TokenResponse>.OkResult(CreateResponse(sessionInfo));
    }

    public async Task<Result<TokenResponse>> CreateAndSave(SessionInfo sessionInfo)
    {
        TokenResponse response = CreateResponse(sessionInfo);
        try
        {
            UserEntry userEntry = sessionInfo.UserEntry;
            response.Token.SessionInfo.UserEntry = null!;
            await _repository.SaveTokenAndSession(response.Token);
            response.Token.SessionInfo.UserEntry = userEntry;
            response.Token.SessionInfo = sessionInfo;
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result<TokenResponse>.ErrorResult("Internal server error", 500);
        }

        return Result<TokenResponse>.OkResult(response);
    }

    public async Task<Result<UserAuthorizeResponse>> UpdateToken(UpdateTokenRequest tokenRequest)
    {
        try
        {
            string hashToken = Hasher.HashKey(tokenRequest.Token);
            Token? oldToken = await _repository.GetByRefreshHash(hashToken);

            Result result = _tokenValidator.ValidateToken(oldToken);
            if (!result.Success)
                return Result<UserAuthorizeResponse>.From(result);

            tokenRequest.Token = string.Empty;
            TokenResponse response = CreateResponse(oldToken!.SessionInfo);

            await _repository.Replace(oldToken, response.Token);

            return Result<UserAuthorizeResponse>.OkResult(response.Response);
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result<UserAuthorizeResponse>.ErrorResult("Internal server error");
        }
    }

    public static TokenResponse CreateResponse(SessionInfo sessionInfo)
    {
        if (sessionInfo.UserEntry is null) throw new NullReferenceException("User entry can't be null");

        (string Token, string TokenHash) refresh = Tokenizer.CreateRefresh();

        string access = Tokenizer.CreateAccess(
            sessionInfo.UserEntry.UniqueName,
            sessionInfo.UserEntryId,
            sessionInfo.Id);

        return new TokenResponse(
            new(refresh.TokenHash, sessionInfo, GetLifeTime()),
            new(access, refresh.Token, sessionInfo.ToDTO())
            );
    }

    private static DateTime GetLifeTime() => DateTime.UtcNow.AddDays(30);
}