using CommonObjects.DTO;
using CommonObjects.Results;

namespace SpoofMessageService.Services;

public interface ISearchService
{
    public Task<Result<List<SearchableEntity>>> SimpleSearchChats(string query, Guid userId);
    public Task<Result<List<SearchableMessage>>> SimpleSearchMessages(string query, Guid userId);
}
