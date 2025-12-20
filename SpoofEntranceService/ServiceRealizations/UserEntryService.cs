using AdditionalHelpers;
using CommonObjects.Requests;
using CommonObjects.Responses;
using CommonObjects.Results;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SecurityLibrary;
using SpoofEntranceService.Models;
using SpoofEntranceService.Services;

namespace SpoofEntranceService.ServiceRealizations
{
    public class UserEntryService(
        SpoofEntranceServiceDbContext context,
        IRepository repository,
        ILoggerService logService,
        ITokenService tokenService,
        ISessionService sessionService
    ) : IUserEntryService
    {
        private readonly IRepository _repository = repository;
        private readonly ILoggerService _logService = logService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly SpoofEntranceServiceDbContext _context = context;
        private const string USER_KEY = "userEntry:";
        private static readonly TimeSpan UserEntryExpiration = TimeSpan.FromMinutes(10);

        public async Task<Result<UserAuthorizeResponse>> Authorization(
            UserAuthorizeRequest request,
            SessionInfo sessionInfo)
        {
            try
            {
                UserEntry? userEntry = await _repository.GetAsync(
                $"{USER_KEY}{request.Login}",
                async () =>
                    await _context.UserEntries.FirstOrDefaultAsync(x => !x.IsDeleted && x.UniqueName == request.Login),
                    UserEntryExpiration);

                if (userEntry is null)
                    return Result<UserAuthorizeResponse>.NotFoundResult("Invalid login");

                if (!Hasher.VerifyPassword(request.Password, userEntry.PasswordHash))
                    return Result<UserAuthorizeResponse>.ErrorResult("Invalid password", 403);

                await _sessionService.StartSession(userEntry, sessionInfo);

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
                UserEntry? user = await _repository.GetAsync(
                    $"{USER_KEY}{request.Login}",
                    async () => await _context.UserEntries.FirstOrDefaultAsync(x => x.UniqueName == request.Login),
                    UserEntryExpiration);

                if (user is { IsDeleted: true })
                    return Result<UserAuthorizeResponse>.ErrorResult("Login is busy");

                user?.UniqueName = "";

                UserEntry newUser = new()
                {
                    UniqueName = request.Login,
                    PasswordHash = Hasher.HashPassword(request.Password)
                };

                List<(string key, UserEntry obj, EntityState state)> entities = [(
                            $"{USER_KEY}{newUser.UniqueName}",
                            newUser,
                            EntityState.Added
                        )];
                if (user is not null)
                    entities.Add((
                        $"{USER_KEY}{newUser.UniqueName}",
                        user,
                        EntityState.Modified));


                await _repository.ChangeStates(_context, entities.ToArray(), UserEntryExpiration);

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
                UserEntry? user = await _repository.GetAsync(
                    $"{USER_KEY}{sessionInfo.UserEntry.UniqueName}",
                    async () => await _context.UserEntries.FirstOrDefaultAsync(x => x.UniqueName == sessionInfo.UserEntry.UniqueName),
                    UserEntryExpiration);
                if (user is null)
                    return Result.NotFoundResult($"User not found");

                await _sessionService.EndSessions(sessionInfo.Id, true);
                await _repository.ChangeState(
                    _context,
                    $"{USER_KEY}{user.UniqueName}",
                    user,
                    EntityState.Deleted,
                    UserEntryExpiration);
                return Result.SuccessResult("Ok");
            }
            catch(Exception ex)
            {
                _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
                return Result.ErrorResult(ex.Message);
            }
        }
    }
}
