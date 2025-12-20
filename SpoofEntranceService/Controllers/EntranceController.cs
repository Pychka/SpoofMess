using CommonObjects.Requests;
using CommonObjects.Responses;
using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpoofEntranceService.Services;

namespace SpoofEntranceService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EntranceController(IUserEntryService userEntryService, ITokenService tokenService) : ControllerBase
{
    private readonly IUserEntryService _userEntryService = userEntryService;
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("Enter")]
    public async ValueTask<IActionResult> Enter(UserAuthorizeRequest request)
    {
        Result<UserAuthorizeResponse> result = await _userEntryService.Authorization(request, new());

        return StatusCode(result.StatusCode, result.Success ? result.Body : result.Error);
    }

    [HttpPost("Registration")]
    public async ValueTask<IActionResult> Enter(RegistrationRequest request)
    {
        Result<UserAuthorizeResponse> result = await _userEntryService.Registration(request, new());

        return StatusCode(result.StatusCode, result.Success ? result.Body : result.Error);
    }

    [Authorize]
    [HttpDelete("Delete")]
    public async ValueTask<IActionResult> Delete()
    {
        throw new NotImplementedException("Need check person");
    }

    [HttpPost("UpdateToken")]
    public async ValueTask<IActionResult> UpdateToken(UpdateTokenRequest request)
    {
        Result<UserAuthorizeResponse> result = await _tokenService.UpdateToken(request);

        return StatusCode(result.StatusCode, result.Success ? result.Body : result.Error);
    }
}
