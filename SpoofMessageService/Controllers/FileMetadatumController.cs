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
public class FileMetadatumController(IFileMetadatumService fileMetadatumService) : ControllerBase
{
    private readonly IFileMetadatumService _fileMetadatumService = fileMetadatumService;

    [HttpGet("get")]
    public async Task<IActionResult> Get(Guid fileId)
    {
        Guid userId = ClaimService.GetUserId(User);
        Result<FileMetadata> result = await _fileMetadatumService.Get(fileId, userId);
        return StatusCode(
           result.StatusCode,
           result.Success
               ? result.Body
               : result.Error
           );
    }
}