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
    public override async Task<Attachment?> GetByIdAsync(Guid id)
    {
        try
        {
            Attachment? entity = await _cache.Get<Attachment>(GetKey(id));
            await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
            entity ??= await context.Attachments.Include(x => x.Message).Include(x => x.FileMetadata).FirstOrDefaultAsync(x => x.Id!.Equals(id));

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }
}
