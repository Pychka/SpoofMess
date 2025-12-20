using CommonObjects.Requests;
using CommonObjects.Results;
using SpoofEntranceService.Models;

namespace SpoofEntranceService.Services;

public interface ISessionService
{

    public Task<Result<List<CommonObjects.DTO.SessionInfo>>> GetSessions(Guid userEntryId, Guid sessionInfoId);

    /// <summary>
    /// End current session
    /// </summary>
    /// <param name="request"></param>
    /// <param name="sessionInfo"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Result> Exit(ExitRequest request, Guid sessionInfoId);

    public Task<Result> StartSession(UserEntry userEntry, SessionInfo sessionInfo);

    /// <summary>
    /// End current session
    /// </summary>
    /// <param name="request"></param>
    /// <param name="sessionInfo"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Result> EndSession(EndSessionRequest request, Guid sessionInfoId);

    /// <summary>
    /// End all(or all without current) sessions
    /// </summary>
    /// <param name="id">Current session id</param>
    /// <param name="withCurrent">End with current</param>
    /// <returns></returns>
    public Task<Result> EndSessions(Guid id, bool withCurrent = false);
}
