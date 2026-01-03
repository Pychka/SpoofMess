using CommonObjects.Requests;
using CommonObjects.Results;

namespace SpoofSettingsService.Services;

public interface IChatUserService
{
    public Task<Result> AddMember(AddMemberRequest request, Guid userId);
    public Task<Result> RemoveMember(DeleteMemberRequest request, Guid userId);
    public Task<Result> ChangeMemberRights();
}
