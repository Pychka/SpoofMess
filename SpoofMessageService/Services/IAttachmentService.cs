using CommonObjects.DTO;
using CommonObjects.Requests.Attachments;
using CommonObjects.Results;

namespace SpoofMessageService.Services;

public interface IAttachmentService
{
    public Task<Result> AddAttachment(AddAttachmentRequest request);

    public Task<Result> RemoveAttachment(Guid id);

    public Task<Result<FileMetadata>> GetToken(byte[] token, Guid userId);
}
