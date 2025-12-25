using AdditionalHelpers;
using CommonObjects.Requests;
using CommonObjects.Results;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofEntranceService.Converters;
using SpoofEntranceService.Models;
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

    public static Result IsInvalidSession(SessionInfo? sessionInfo)
    {
        if (sessionInfo is null)
            return Result.NotFoundResult($"Not found your session");

        if (sessionInfo.IsDeleted || !sessionInfo.IsActive)
            return Result.ErrorResult("Session is disabled or is deleted", 400);

        return Result.SuccessResult();
    }

    public static bool IsSessionTooNew(SessionInfo sessionInfo, DateTime now) =>
        sessionInfo.CreatedAt.Date >= now.AddDays(-7).Date;

    private async ValueTask<SessionInfo?> GetSessionInfo(Guid sessionId) =>
        await _repository.GetAsync(
                $"{SESSION_KEY}{sessionId}",
                async () => await _context.SessionInfos.FirstOrDefaultAsync(x => x.Id == sessionId),
                SessionExpiration);
    
    private async Task ChangeState(SessionInfo sessionInfo) =>
        await _repository.ChangeState(
                _context,
                $"{SESSION_KEY}{sessionInfo.Id}",
                sessionInfo,
                EntityState.Modified);

    private async ValueTask<List<SessionInfo>?> GetSessionInfos(Guid sessionId, Guid userId)
    {
        return await _repository.GetAsync(
                $"{USER_KEY}sessions:{userId}",
                async () => await _context.SessionInfos
                    .Where(x => x.UserEntryId == userId && x.IsActive && !x.IsDeleted && x.Id != sessionId).ToListAsync(),
                UserEntryExpiration);
    }

    public async Task<Result> EndSession(EndSessionRequest request, Guid sessionInfoId)
    {
        try
        {
            SessionInfo? currentSessionInfo = await GetSessionInfo(sessionInfoId);
            Result result = IsInvalidSession(currentSessionInfo);
            if(!result.Success)
                return result;

            if (IsSessionTooNew(currentSessionInfo!, DateTime.UtcNow))
                return Result.ErrorResult("No trust", 403);

            SessionInfo? deletedSessionInfo = await GetSessionInfo(request.SessionId);

            result = IsInvalidSession(deletedSessionInfo);
            if (!result.Success)
                return result;

            deletedSessionInfo!.IsActive = false;
            deletedSessionInfo.IsDeleted = true;

            await ChangeState(deletedSessionInfo);

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
            SessionInfo? sessionInfo = await GetSessionInfo(sessionInfoId);

            Result result = IsInvalidSession(sessionInfo);
            if (!result.Success)
                return result;

            sessionInfo!.IsActive = false;
            sessionInfo.IsDeleted = true;

            await ChangeState(sessionInfo);

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
            List<SessionInfo>? sessions = await GetSessionInfos(sessionInfoId, userId);

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

            await ChangeState(sessionInfo);

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
            SessionInfo? session = await GetSessionInfo(id);

            Result result = IsInvalidSession(session);
            if (!result.Success)
                return result;

            if (IsSessionTooNew(session!, DateTime.UtcNow))
                return Result.ErrorResult("Not trust", 403);

            List<SessionInfo>? sessionInfos = await GetSessionInfos(id, session!.UserEntryId);

            if (sessionInfos is null)
                return Result.NotFoundResult($"Not found session for {session.UserEntryId}");

            List<(string key, SessionInfo obj, EntityState state)> entities = [.. sessionInfos.Select(x =>
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
