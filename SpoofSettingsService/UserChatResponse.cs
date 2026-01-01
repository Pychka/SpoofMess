using CommonObjects.Results;
using SpoofSettingsService.Models;

namespace SpoofSettingsService;

public readonly record struct UserChatResult(
        User? User,
        Chat? Chat,
        Result Result = null!
    );
