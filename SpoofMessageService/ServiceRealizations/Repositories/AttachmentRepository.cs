using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations.Repositories;

public class AttachmentRepository(
    ICacheService cache,
    IDbContextFactory<SpoofMessageServiceContext> factory,
    IProcessQueueTasksService processQueueTasks)
    : CachedSoftDeletableIdentifiedFactoryRepository<Attachment, Guid, SpoofMessageServiceContext>(
        cache,
        factory,
        processQueueTasks), IAttachmentRepository
{
}
