using SpoofEntranceService.Models;

namespace SpoofEntranceService.Converters;

internal static class SessionConverter
{
    internal static CommonObjects.DTO.SessionInfo ToDTO(this SessionInfo sessionInfo) =>
        new()
        {
            CreatedAt = sessionInfo.CreatedAt,
            IsActive = sessionInfo.IsActive,
            LastActivityAt = sessionInfo.LastActivityAt,
            UserAgent = sessionInfo.UserAgent,
            DeviceId = sessionInfo.DeviceId,
            DeviceName = sessionInfo.DeviceName,
            Id = sessionInfo.Id,
            Platform = sessionInfo.Platform,
            UserEntryId = sessionInfo.UserEntryId,
        };
}
