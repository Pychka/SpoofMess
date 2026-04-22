using AdditionalHelpers.Services;
using CommonObjects.Requests.Attachments;
using CommonObjects.Results;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations;

public class AttachmentService(
    ILoggerService loggerService,
    IAttachmentRepository attachmentRepository
    ) : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
    private readonly ILoggerService _loggerService = loggerService;
    public async Task<Result> AddAttachment(AddAttachmentRequest request)
    {
        try
        {
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("Internal server error");
        }
    }

    public async Task<Result> RemoveAttachment(Guid id)
    {
        try
        {
            await _attachmentRepository.SoftExecuteDelete(id);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("Internal server error");
        }
    }
}
