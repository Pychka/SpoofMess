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
public class SearchController(ISearchService searchService) : ControllerBase
{
    private readonly ISearchService _searchService = searchService;
    [HttpGet("simple-search-chats")]
    public async Task<IActionResult> SimpleSearchChats(string query)
    {
        Guid userId = ClaimService.GetUserId(User);
        Result<List<SearchableEntity>> result = await _searchService.SimpleSearchChats(query, userId);
        return StatusCode(result.StatusCode,
                          result.Success ? result.Body : result.Error ?? result.Message);
    }
    [HttpGet("simple-search-messages")]
    public async Task<IActionResult> SimpleSearchMessages(string query)
    {
        Guid userId = ClaimService.GetUserId(User);
        Result<List<SearchableMessage>> result = await _searchService.SimpleSearchMessages(query, userId);
        return StatusCode(result.StatusCode,
                          result.Success ? result.Body : result.Error ?? result.Message);
    }
}