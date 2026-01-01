using AdditionalHelpers;
using CommonObjects.Requests;
using CommonObjects.Responses;
using CommonObjects.Results;
using Microsoft.EntityFrameworkCore;
using SecurityLibrary;
using SpoofEntranceService.Converters;
using SpoofEntranceService.Models;
using SpoofEntranceService.Repositories;
using SpoofEntranceService.Services;
using SpoofEntranceService.Validators;

namespace SpoofEntranceService.ServiceRealizations;

public class TokenService(TokenRepository repository, TokenValidator tokenValidator, ILoggerService loger) : ITokenService
{
    private readonly TokenRepository _repository = repository;
    private readonly ILoggerService _logService = loger;
    private readonly TokenValidator _tokenValidator = tokenValidator;

    public async Task<Result<UserAuthorizeResponse>> Create(SessionInfo sessionInfo)
    {
        TokenResponse response = CreateResponse(sessionInfo);

        try
        {
            await _repository.Add(response.Token);
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result<UserAuthorizeResponse>.ErrorResult("Internal server error", 500);
        }

        return Result<UserAuthorizeResponse>.OkResult(response.Response);
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

    private static DateTime GetLifeTime() => DateTime.UtcNow.AddDays(30);

    public static TokenResponse CreateResponse(SessionInfo sessionInfo)
    {
        if (sessionInfo.UserEntry is null) throw new NullReferenceException("User entry can't be null");

        (string Token, string TokenHash) refresh = Tokenizer.CreateRefresh();

        string access = Tokenizer.CreateAccess(
            sessionInfo.UserEntry.UniqueName,
            sessionInfo.UserEntryId,
            sessionInfo.Id);

        return new TokenResponse(
            new(refresh.TokenHash, sessionInfo.Id, GetLifeTime()),
            new(access, refresh.Token, sessionInfo.ToDTO())
            );
    }
}