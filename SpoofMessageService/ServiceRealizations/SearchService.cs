using AdditionalHelpers.Services;
using CommonObjects.Results;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations;

public class SearchService(ISearchRepository searchRepository, ILoggerService loggerService) : ISearchService
{
    private readonly ISearchRepository _searchRepository = searchRepository;
    private readonly ILoggerService _loggerService = loggerService;
    public async Task<Result<List<SearchableEntity>>> Search(string query, Guid userId)
    {
        try
        {
            List<SearchableEntity> searchableEntities = await _searchRepository.GetChats(query, userId);
            if (searchableEntities.Count > 0)
                return Result<List<SearchableEntity>>.OkResult(searchableEntities);
            return Result<List<SearchableEntity>>.NotFoundResult("No results");
        }
        catch(Exception ex)
        {
            _loggerService.Error(ex.Message, ex);
            return Result<List<SearchableEntity>>.InternalServerError();
        }
    }
}
