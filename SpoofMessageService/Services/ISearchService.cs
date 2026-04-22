using CommonObjects.Results;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services;

public interface ISearchService
{
    public Task<Result<List<SearchableEntity>>> Search(string query, Guid userId);
}
