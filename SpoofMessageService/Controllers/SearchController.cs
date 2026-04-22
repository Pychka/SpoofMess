using CommonObjects.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityLibrary;
using SpoofMessageService.Models;
using SpoofMessageService.Services;

namespace SpoofMessageService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    private readonly ISearchService _searchService = searchService;
    [HttpGet]
    public async Task<IActionResult> SearchChats(string query)
    {
        Guid userId = ClaimService.GetUserId(User);
        Result<List<SearchableEntity>> result = await _searchService.Search(query, userId);
        return StatusCode(result.StatusCode,
                          result.Success ? result.Body : result.Error ?? result.Message);
    }
    
}
