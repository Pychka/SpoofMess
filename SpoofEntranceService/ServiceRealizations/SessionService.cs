using CommonObjects.Requests;
using CommonObjects.Results;
using Microsoft.EntityFrameworkCore;
using SpoofEntranceService.Converters;
using SpoofEntranceService.Models;
using AdditionalHelpers;
using DataHelpers.Services;
using SpoofEntranceService.Services;

namespace SpoofEntranceService.ServiceRealizations;

public class SessionService(
        SpoofEntranceServiceDbContext context,
        IRepository repository,
        ILoggerService logService
    ) : ISessionService
{
    private readonly IRepository _repository = repository;
    private readonly ILoggerService _logService = logService;
    private readonly SpoofEntranceServiceDbContext _context = context;
    private const string USER_KEY = "userEntry:";
    private const string SESSION_KEY = "session:";
    private static readonly TimeSpan SessionExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan UserEntryExpiration = TimeSpan.FromMinutes(10);


    public async Task<Result> EndSession(EndSessionRequest request, Guid sessionInfoId)
    {
        try
        {
            SessionInfo? currentSessionInfo = await _repository.GetAsync(
                $"{SESSION_KEY}{sessionInfoId}",
                async () => await _context.SessionInfos.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == sessionInfoId),
                SessionExpiration);

            if (currentSessionInfo is null
                || currentSessionInfo.CreatedAt > DateTime.UtcNow.AddDays(-7))
                return Result.ErrorResult("No trust", 403);

            SessionInfo? deletedSessionInfo = await _repository.GetAsync(
                $"{SESSION_KEY}{request.SessionId}",
                async () => await _context.SessionInfos.FirstOrDefaultAsync(x => x.Id == request.SessionId),
                SessionExpiration);

            if (deletedSessionInfo is null || deletedSessionInfo.IsDeleted || !deletedSessionInfo.IsActive)
                return Result.NotFoundResult("Invalid session");

            deletedSessionInfo.IsActive = false;
            deletedSessionInfo.IsDeleted = true;

            await _repository.ChangeState(
                _context,
                $"{SESSION_KEY}{sessionInfoId}",
                deletedSessionInfo,
                EntityState.Modified);
            return Result.DeletedResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
            return Result.ErrorResult(ex.Message);
        }
    }

    public async Task<Result> Exit(ExitRequest request, Guid sessionInfoId)
    {
        try
        {

            SessionInfo? sessionInfo = await _repository.GetAsync(
                $"{SESSION_KEY}{sessionInfoId}",
                async () => await _context.SessionInfos.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == sessionInfoId),
                SessionExpiration);

            if (sessionInfo is null)
                return Result.NotFoundResult("Invalid session");

            sessionInfo.IsActive = false;
            sessionInfo.IsDeleted = true;

            await _repository.ChangeState(
                _context,
                $"{SESSION_KEY}{sessionInfo.Id}",
                sessionInfo,
                EntityState.Modified);
            return Result.DeletedResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
            return Result.ErrorResult(ex.Message);
        }
    }

    public async Task<Result<List<CommonObjects.DTO.SessionInfo>>> GetSessions(Guid userId, Guid sessionInfoId)
    {
        try
        {
            List<SessionInfo>? sessions = await _repository.GetAsync(
                $"{USER_KEY}sessions:{userId}",
                async () =>
                    await _context.SessionInfos
                    .Where(x => !x.IsDeleted && x.IsActive && x.UserEntryId == userId && x.Id != sessionInfoId)
                    .ToListAsync(),
                UserEntryExpiration);

            if (sessions is null)
                return Result<List<CommonObjects.DTO.SessionInfo>>.NotFoundResult("Invalid id");

            return Result<List<CommonObjects.DTO.SessionInfo>>.SuccessResult("Ok", [.. sessions.Select(x => x.ToDTO())]);
        }
        catch (Exception ex)
        {
            _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
            return Result<List<CommonObjects.DTO.SessionInfo>>.ErrorResult(ex.Message);
        }
    }

    public async Task<Result> StartSession(UserEntry userEntry, SessionInfo sessionInfo)
    {
        try
        {
            sessionInfo.IsActive = true;
            sessionInfo.UserEntry = userEntry;

            await _repository.ChangeState(
                _context,
                $"{SESSION_KEY}{sessionInfo.Id}",
                sessionInfo,
                EntityState.Modified,
                SessionExpiration);
            return Result.SuccessResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
            return Result.ErrorResult(ex.Message);
        }
    }

    public async Task<Result> EndSessions(Guid id, bool withCurrent = false)
    {
        try
        {
            SessionInfo? session = await _repository.GetAsync(
                $"{SESSION_KEY}{id}",
                async () => await _context.SessionInfos
                .Include(x => x.UserEntry)
                .ThenInclude(
                    x => x.SessionInfos
                    .Where(x => !x.IsDeleted && x.Id != id))
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id),
                SessionExpiration);
            if (session is null)
                return Result.NotFoundResult($"Session with {id} not contains");

            if (session is null || session.CreatedAt > DateTime.UtcNow.AddDays(-7))
                return Result.ErrorResult("Not trust", 403);
            List<(string key, SessionInfo obj, EntityState state)> entities = [.. session.UserEntry.SessionInfos.Select(x =>
                ($"{SESSION_KEY}{x.Id}", x, EntityState.Deleted))];

            if (withCurrent)
                entities.Add(($"{SESSION_KEY}{session.Id}", session, EntityState.Deleted));

            await _repository.ChangeStates(_context, entities.ToArray(), SessionExpiration);

            return Result.SuccessResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Log(AdditionalHelpers.LogLevel.Error, "Error", ex);
            return Result.ErrorResult("Error");
        }
    }
}
