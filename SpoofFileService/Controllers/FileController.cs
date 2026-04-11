using CommonObjects.Requests.Files;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SecurityLibrary;
using SpoofFileService.Services;

namespace SpoofFileService.Controllers;


[Authorize]
[ApiController]
[Route("api/v3/[controller]")]
public class FileController(IFileService fileService) : ControllerBase
{
    private readonly IFileService _fileService = fileService;

    [HttpPost("first-check")]
    public async Task<IActionResult> CheckL1(FingerprintExistL1L2 request)
    {
        Result result = await _fileService.ExistL1(request);
        return StatusCode(result.StatusCode, result.Success
            ? "L1 access granted"
            : result.Error);
    }
    [HttpPost("full-check")]
    public async Task<IActionResult> CheckL3(FingerprintExistL3 request)
    {
        Guid userId = ClaimService.GetUserId(User);
        Result<byte[]> result = await _fileService.ExistL3(request, userId);
        return StatusCode(result.StatusCode, result.Success
            ? result.Body
            : result.Error);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(byte[] token)
    {
        Guid userId = ClaimService.GetUserId(User);
        Result<FileStream> result = await _fileService.GetFile(token, userId);

        return StatusCode(
            result.StatusCode,
            result.Success
                ? result.Body
                : result.Error
                );
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save()
    {
        if (Request.ContentType == null || !Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Неверный тип контента");

        Guid userId = ClaimService.GetUserId(User);
        MultipartReader reader = new(
            Request.GetMultipartBoundary(),
            Request.Body);
        MultipartSection? section = await reader.ReadNextSectionAsync();

        while (section is not null)
        {
            if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
            {
                if (contentDisposition.IsFileDisposition())
                {
                    Result<byte[]> result = await _fileService.SaveFile(section.Body, userId);

                    return StatusCode(
                        result.StatusCode,
                        result.Success
                            ? result.Body
                            : result.Error
                            );
                }
            }

            section = await reader.ReadNextSectionAsync();
        }
        return BadRequest("Invalid body");
    }
}
