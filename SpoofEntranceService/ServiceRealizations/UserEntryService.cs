using AdditionalHelpers.Services;
using CommonObjects.Requests;
using CommonObjects.Responses;
using CommonObjects.Results;
using SecurityLibrary;
using SpoofEntranceService.Models;
using SpoofEntranceService.Services;
using SpoofEntranceService.Services.Publishers;
using SpoofEntranceService.Services.Repositories;
using SpoofEntranceService.Services.Validators;

namespace SpoofEntranceService.ServiceRealizations;

public class UserEntryService(
    IUserEntryRepository repository,
    IUserPublisherService userPublisherService,
    IUserEntryValidator validator, 
    ILoggerService logService,
    ITokenService tokenService, 
    ISessionService sessionService) : IUserEntryService
{
    private readonly IUserPublisherService _userPublisherService = userPublisherService;
    private readonly IUserEntryRepository _repository = repository;
    private readonly ISessionService _sessionService = sessionService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserEntryValidator _validator = validator;
    private readonly ILoggerService _logService = logService;

    public async Task<Result<UserAuthorizeResponse>> Authorization(HttpContext context,
        UserAuthorizeRequest request,
        SessionInfo sessionInfo)
    {
        try
        {
            UserEntry? user = await _repository.GetByLogin(request.Login);

            Result result = _validator.IsAvailable(user);
            if (!result.Success)
                return Result<UserAuthorizeResponse>.From(result);

            if (!Hasher.VerifyPassword(request.Password, user!.PasswordHash))
                return Result<UserAuthorizeResponse>.Forbidden("Invalid password");

            await _sessionService.StartSession(context, user, sessionInfo);
            Result<TokenResponse> response = await _tokenService.CreateAndSave(sessionInfo);

            sessionInfo.UserEntry = user;
            return Result<UserAuthorizeResponse>.OkResult(response.Body.Response);
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result<UserAuthorizeResponse>.ErrorResult(ex.Message);
        }
    }
    public async Task<Result<UserAuthorizeResponse>> Registration(HttpContext context, RegistrationRequest request, SessionInfo sessionInfo)
    {
        try
        {
            UserEntry? user = await _repository.GetByLogin(request.Login);
            Result userResult = _validator.HisIsActive(user);
            if (!userResult.Success)
                return Result<UserAuthorizeResponse>.From(userResult);

            Task<Guid> id = Task.Run(Guid.CreateVersion7);
            Task<string> password = Task.Run(() => Hasher.HashPassword(request.Password));

            await Task.WhenAll(id, password);

            UserEntry newUser = new()
            {
                Id = id.Result,
                UniqueName = request.Login,
                PasswordHash = password.Result,
            };

            await _repository.Change(user);
            await _sessionService.StartSession(context, newUser, sessionInfo);
            _ = Task.Run(() => _userPublisherService.Create(new(newUser.Id, request.Name, newUser.UniqueName)));

            Result<TokenResponse> result = await _tokenService.Create(sessionInfo);
            if (!result.Success)
                return Result<UserAuthorizeResponse>.From(result);

            await _repository.Create(newUser, sessionInfo, result.Body.Token);
            return Result<UserAuthorizeResponse>.OkResult(result.Body.Response);
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result<UserAuthorizeResponse>.ErrorResult(ex.Message);
        }
    }

    public async Task<Result> Delete(SessionInfo sessionInfo)
    {
        try
        {
            UserEntry? user = await _repository.GetByIdAsync(sessionInfo.UserEntryId);

            Result result = _validator.IsAvailable(user);
            if (!result.Success)
                return result;

            await _sessionService.EndSessions(sessionInfo.Id, true);
            await _repository.SoftDeleteAsync(user!);
            return Result.OkResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result.ErrorResult(ex.Message);
        }
    }
    public async Task Confirm(Guid userId)
    {
        try
        {
            UserEntry? user = await _repository.GetByIdAsync(userId);

            Result result = _validator.IsAvailable(user);
            if (!result.Success)
                return;
            user!.IsDeleted = false;

            await _repository.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logService.Error("Database error", ex);
        }
    }

    public async Task Error(Guid userId) =>
        await ChangeStatus(userId, true);

    public async Task Delete(Guid userId)
    {
        try
        {
            await _repository.SoftDeleteAsync(userId);
        }
        catch (Exception ex)
        {
            _logService.Error("Database error", ex);
        }
    }

    [Obsolete("Need change logic")]
    public async Task ChangeStatus(Guid userId, bool isDeleted)
    {
        try
        {
            UserEntry? user = await _repository.GetByIdAsync(userId);

            Result result = _validator.IsAvailable(user);
            if (!result.Success)
                return;
            user!.IsDeleted = isDeleted;

            await _repository.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
        }
    }
}