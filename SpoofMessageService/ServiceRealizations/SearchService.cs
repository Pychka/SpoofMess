using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Results;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations;

public class SearchService(ISearchRepository searchRepository, ILoggerService loggerService) : ISearchService
{
    private readonly ISearchRepository _searchRepository = searchRepository;
    private readonly ILoggerService _loggerService = loggerService;
    public async Task<Result<List<SearchableEntity>>> SimpleSearchChats(string query, Guid userId)
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

    public async Task<Result<List<SearchableMessage>>> SimpleSearchMessages(string query, Guid userId)
    {
        try
        {
            List<SearchableMessage> searchableEntities = await _searchRepository.GetMessages(query, userId);
            if (searchableEntities.Count > 0)
                return Result<List<SearchableMessage>>.OkResult(searchableEntities);
            return Result<List<SearchableMessage>>.NotFoundResult("No results");
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message, ex);
            return Result<List<SearchableMessage>>.InternalServerError();
        }
    }
}
