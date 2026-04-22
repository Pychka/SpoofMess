using DataSaveHelpers.Services.Repositories;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Repositories;

public interface IAttachmentRepository : ISoftDeletableIdentifiedRepository<Attachment, Guid>
{
}
