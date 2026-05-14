using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Requests.Attachments;
using CommonObjects.Results;
using SecurityLibrary.Tokens;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Repositories;
using SpoofMessageService.Services.Validators;

namespace SpoofMessageService.ServiceRealizations;

public class AttachmentService(
    ILoggerService loggerService,
    IAttachmentRepository attachmentRepository,
    IAttachmentAccessTokenService attachmentTokenService,
    IAttachmentValidator attachmentValidator,
    IChatUserService chatUserService,
    IFileMetadatumService fileMetadatumService
    ) : IAttachmentService
{
    private readonly IFileMetadatumService _fileMetadatumService = fileMetadatumService;
    private readonly IAttachmentValidator _attachmentValidator = attachmentValidator;
    private readonly IAttachmentAccessTokenService _attachmentTokenService = attachmentTokenService;
    private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IChatUserService _chatUserService = chatUserService;
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

    public async Task<Result<FileMetadata>> GetToken(byte[] token, Guid userId)
    {
        try
        {
            if (!_attachmentTokenService.IsValid(token, out Guid attachmentId))
                return Result<FileMetadata>.Forbidden("Unvalid token");
            Models.Attachment? attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
            Result result = _attachmentValidator.IsAvailable(attachment);
            if (!result.Success)
                return Result<FileMetadata>.From(result);
            Result<ChatUser> resultChatUser = await _chatUserService.GetMember(attachment!.Message.ChatId, userId);

            if (!resultChatUser.Success)
                return Result<FileMetadata>.From(resultChatUser);

            return await _fileMetadatumService.Get(attachment, userId);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<FileMetadata>.ErrorResult("Internal server error");
        }
    }
}
