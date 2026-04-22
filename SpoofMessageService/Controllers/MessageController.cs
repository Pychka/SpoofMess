using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityLibrary;
using SpoofMessageService.Services;

namespace SpoofMessageService.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(IMessageService messageService) : ControllerBase
{
    private readonly IMessageService _messageService = messageService;

    [HttpPost]
    public async Task<IActionResult> Send(CreateMessageRequest request)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result<MessageDTO> result = await _messageService.SendMessage(request, userId);
        return StatusCode(
            result.StatusCode,
            result.Success 
                ? result.Message
                : result.Error
            );
    }
    
    [HttpGet("get-after")]
    public async Task<IActionResult> GetAfterDate(Guid chatId, DateTime date, int take = 50)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result<List<MessageDTO>> result = await _messageService.GetMessagesAfterDate(
                chatId,
                userId,
                date.ToUniversalTime(),
                take
            );
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Body
                : result.Error
            );
    }

    [HttpGet("get-before")]
    public async Task<IActionResult> GetBeforeDate(Guid chatId, DateTime date, int take = 50)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result<List<MessageDTO>> result = await _messageService.GetMessagesBeforeDate(
                chatId, 
                userId, 
                date.ToUniversalTime(), 
                take
            );
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Body
                : result.Error
            );
    }

    [HttpGet("get-skiped")]
    public async Task<IActionResult> GetSkipped(DateTime after, int take = 50)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result<List<MessageDTO>> result = await _messageService.GetSkippedMessages(
                userId,
                after.ToUniversalTime(),
                take
            );
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Body
                : result.Error
            );
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteMessage(Guid messageId, Guid chatId)
    {
        Guid userId = ClaimService.GetUserId(User);

        Result result = await _messageService.DeleteMessage(
                messageId, 
                chatId,
                userId
            );
        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Message
                : result.Error
            );
    }
}
