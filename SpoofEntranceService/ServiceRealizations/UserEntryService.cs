using AdditionalHelpers;
using CommonObjects.Requests;
using CommonObjects.Responses;
using CommonObjects.Results;
using SecurityLibrary;
using SpoofEntranceService.Models;
using SpoofEntranceService.Repositories;
using SpoofEntranceService.Services;
using SpoofEntranceService.Validators;

namespace SpoofEntranceService.ServiceRealizations
{
    public class UserEntryService(
        UserRepository repository,
        UserEntryValidator validator,
        ILoggerService logService,
        ITokenService tokenService,
        ISessionService sessionService
    ) : IUserEntryService
    {
        private readonly UserRepository _repository = repository;
        private readonly UserEntryValidator _validator = validator;
        private readonly ILoggerService _logService = logService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<Result<UserAuthorizeResponse>> Authorization(
            UserAuthorizeRequest request,
            SessionInfo sessionInfo)
        {
            try
            {
                UserEntry? user = await _repository.GetByLogin(request.Login);

                Result result = _validator.IsActive(user);
                if (!result.Success)
                    return Result<UserAuthorizeResponse>.From(result);

                if (!Hasher.VerifyPassword(request.Password, user!.PasswordHash))
                    return Result<UserAuthorizeResponse>.ErrorResult("Invalid password", 403);

                await _sessionService.StartSession(user, sessionInfo);

                return await _tokenService.Create(sessionInfo);
            }
            catch (Exception ex)
            {
                _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
                return Result<UserAuthorizeResponse>.ErrorResult(ex.Message);
            }
        }

        public async Task<Result<UserAuthorizeResponse>> Registration(RegistrationRequest request, SessionInfo sessionInfo)
        {
            try
            {
                UserEntry? user = await _repository.GetByLogin(request.Login);

                if (user is { IsDeleted: false })
                    return Result<UserAuthorizeResponse>.ErrorResult("Login is busy");

                UserEntry newUser = new()
                {
                    UniqueName = request.Login,
                    PasswordHash = Hasher.HashPassword(request.Password)
                };

                await _repository.Change(newUser, user);

                await _sessionService.StartSession(newUser, sessionInfo);

                return await _tokenService.Create(sessionInfo);
            }
            catch (Exception ex)
            {
                _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
                return Result<UserAuthorizeResponse>.ErrorResult(ex.Message);
            }
        }

        public async Task<Result> Delete(SessionInfo sessionInfo)
        {
            try
            {
                UserEntry? user = await _repository.GetByIdAsync(sessionInfo.UserEntryId);

                Result result = _validator.IsActive(user);
                if (!result.Success)
                    return result;

                await _sessionService.EndSessions(sessionInfo.Id, true);
                await _repository.SoftDeleteAsync(user!);
                return Result.OkResult("Ok");
            }
            catch(Exception ex)
            {
                _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
                return Result.ErrorResult(ex.Message);
            }
        }
    }
}
