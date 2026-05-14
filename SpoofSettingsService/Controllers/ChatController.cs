using CommonObjects.DTO;
using CommonObjects.Requests;
using CommonObjects.Requests.Changes;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityLibrary;
using SpoofSettingsService.Services;

namespace SpoofSettingsService.Controllers;

[Authorize]
[ApiController]
[Route("api/v2/[controller]")]
public class ChatController(IChatService chatService) : ControllerBase
{
    private readonly IChatService _chatService = chatService;

    [HttpPatch("ChangeSettings")]
    public async Task<IActionResult> ChangeSettings(ChangeChatSettingsRequest request)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result result = await _chatService.ChangeSettings(request, userId);
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Message
                : result.Error
                );
    }


    [HttpGet("Get")]
    public async Task<IActionResult> GetChat(Guid chatId)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result<ChatDTO> result = await _chatService.GetChat(userId, chatId);
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Body
                : result.Error
                );
    }


    [HttpPost("CreateChat")]
    public async Task<IActionResult> CreateChat(CreateChatRequest request)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result<ChatDTO> result = await _chatService.CreateChat(request, userId);
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Body 
                : result.Error
                );
    }


    [HttpDelete("DeleteChat")]
    public async Task<IActionResult> DeleteChat(Guid chatId)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result result = await _chatService.DeleteChat(chatId, userId);
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Message
                : result.Error
                );
    }
}