using CommonObjects.DTO;
using CommonObjects.Requests.Avatars;
using CommonObjects.Responses;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityLibrary;
using SpoofSettingsService.ServiceRealizations;
using SpoofSettingsService.Services;

namespace SpoofSettingsService.Controllers;

[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
public class ChatAvatarController(IChatAvatarService chatAvatarService) : ControllerBase
{
    private readonly IChatAvatarService _chatAvatarService = chatAvatarService;

    [HttpPost("Set")]
    public async Task<IActionResult> Set(SetChatAvatarRequest request)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result result = await _chatAvatarService.SetAvatar(request, userId);
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Message
                : result.Error
                );
    }


    [HttpPost("Get")]
    public async Task<IActionResult> Get(byte[] accessToken)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result<AvatarResponse> result = await _chatAvatarService.GetAvatar(accessToken, userId);
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Body
                : result.Error
                );
    }
}
