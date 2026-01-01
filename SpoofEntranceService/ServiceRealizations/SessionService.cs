using AdditionalHelpers;
using CommonObjects.Requests;
using CommonObjects.Results;
using Microsoft.EntityFrameworkCore;
using SpoofEntranceService.Converters;
using SpoofEntranceService.Models;
using SpoofEntranceService.Repositories;
using SpoofEntranceService.Services;
using SpoofEntranceService.Validators;

namespace SpoofEntranceService.ServiceRealizations;

public class SessionService(
        SessionRepository sessionRepository,
        SessionValidator sessionValidator,
        ILoggerService logService
    ) : ISessionService
{
    private readonly ILoggerService _logService = logService;
    private readonly SessionRepository _sessionRepository = sessionRepository;
    private readonly SessionValidator _sessionValidator = sessionValidator;

    public async Task<Result> EndSession(EndSessionRequest request, Guid sessionInfoId)
    {
        try
        {
            SessionInfo? currentSessionInfo = await _sessionRepository.GetByIdAsync(sessionInfoId);
            Result result = _sessionValidator.ValidateTrustSession(currentSessionInfo);
            if(!result.Success)
                return result;

            SessionInfo? deletedSessionInfo = await _sessionRepository.GetByIdAsync(request.SessionId);

            result = _sessionValidator.IsInvalidSession(deletedSessionInfo);
            if (!result.Success)
                return result;

            await _sessionRepository.SoftDelete(deletedSessionInfo!);

            return Result.DeletedResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result.ErrorResult(ex.Message);
        }
    }

    public async Task<Result> Exit(ExitRequest request, Guid sessionInfoId)
    {
        try
        {
            SessionInfo? sessionInfo = await _sessionRepository.GetByIdAsync(sessionInfoId);

            Result result = _sessionValidator.IsInvalidSession(sessionInfo);
            if (!result.Success)
                return result;

            await _sessionRepository.SoftDelete(sessionInfo!);

            return Result.DeletedResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result.ErrorResult(ex.Message);
        }
    }

    public async Task<Result<List<CommonObjects.DTO.SessionInfo>>> GetSessions(Guid userId, Guid sessionInfoId)
    {
        try
        {
            List<SessionInfo>? sessions = await _sessionRepository.GetSessionsByUserId(userId);

            if (sessions is null)
                return Result<List<CommonObjects.DTO.SessionInfo>>.NotFoundResult("Invalid id");

            return Result<List<CommonObjects.DTO.SessionInfo>>.OkResult([.. sessions.Select(x => x.ToDTO())]);
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result<List<CommonObjects.DTO.SessionInfo>>.ErrorResult(ex.Message);
        }
    }

    public async Task<Result> StartSession(UserEntry userEntry, SessionInfo sessionInfo)
    {
        try
        {
            sessionInfo.Id = Guid.CreateVersion7();
            sessionInfo.IsActive = true;
            sessionInfo.UserEntry = userEntry;
            sessionInfo.DeviceId = Guid.CreateVersion7().ToString();
            await _sessionRepository.AddAsync(sessionInfo);

            return Result.OkResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result.ErrorResult(ex.Message);
        }
    }

    public async Task<Result> EndSessions(Guid id, bool withCurrent = false)
    {
        try
        {
            SessionInfo? session = await _sessionRepository.GetByIdAsync(id);

            Result result = _sessionValidator.ValidateTrustSession(session);
            if (!result.Success)
                return result;

            await _sessionRepository.SoftDeleteSessionsByUserId(session!.UserEntryId, withCurrent, session.Id);

            return Result.OkResult("Ok");
        }
        catch (Exception ex)
        {
            _logService.Error("Error", ex);
            return Result.ErrorResult("Error");
        }
    }
}