using CommonObjects.DTO;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityLibrary;
using SpoofMessageService.Services;

namespace SpoofMessageService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttachmentController(IAttachmentService attachmentService) : ControllerBase
{
    private readonly IAttachmentService _attachmentService = attachmentService;
    [HttpPost("get-token")]
    public async Task<IActionResult> GetToken(byte[] token)
    {
        Guid userId = ClaimService.GetUserId(User);
        Result<FileMetadata> result = await _attachmentService.GetToken(token, userId);
        return StatusCode(result.StatusCode,
                          result.Success ? result.Body : result.Error ?? result.Message);
    }

}
