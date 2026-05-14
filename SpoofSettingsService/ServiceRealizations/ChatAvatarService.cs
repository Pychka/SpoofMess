using AdditionalHelpers.Services;
using CommonObjects.Requests.Avatars;
using CommonObjects.Responses;
using CommonObjects.Results;
using RuleRoleHelper;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services;
using SpoofSettingsService.Services.MessageBrokers;
using SpoofSettingsService.Services.Repositories;
using SpoofSettingsService.Services.Validators;
using SpoofSettingsService.Setters;

namespace SpoofSettingsService.ServiceRealizations;

public class ChatAvatarService(
        ILoggerService loggerService,
        IChatAvatarRepository chatAvatarRepository,
        IFileMetadatumService fileMetadatumService,
        IChatAvatarValidator chatAvatarValidator,
        IRuleService ruleService,
        IChatAvatarPublisherService chatAvatarPublisherService,
        IFileTokenService fileTokenService,
        IChatService chatService,
        IAttachmentAccessTokenService attachmentAccessTokenService
    ) : IChatAvatarService
{
    private readonly IFileMetadatumService _fileMetadatumService = fileMetadatumService;
    private readonly IChatAvatarPublisherService _chatAvatarPublisherService = chatAvatarPublisherService;
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly IChatService _chatService = chatService;
    private readonly IAttachmentAccessTokenService _attachmentAccessTokenService = attachmentAccessTokenService;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IChatAvatarRepository _chatAvatarRepository = chatAvatarRepository;
    private readonly IChatAvatarValidator _chatAvatarValidator = chatAvatarValidator;
    private readonly IRuleService _ruleService = ruleService;

    public async Task<Result<AvatarResponse>> GetAvatar(byte[] accessToken, Guid userId)
    {
        try
        {
            if (!_attachmentAccessTokenService.IsValid(accessToken, out Guid avatarId))
                return Result<AvatarResponse>.Forbidden("Invaid access token");

            ChatAvatar? avatar = await _chatAvatarRepository.GetByIdAsync(avatarId);
            Result result = _chatAvatarValidator.IsAvailable(avatar);
            if (!result.Success)
                return Result<AvatarResponse>.From(result);

            return Result<AvatarResponse>.OkResult(
                new()
                {
                    AvatarTokenAccess = _attachmentAccessTokenService.CreateToken(avatar!.Id),
                    FileMetadata = avatar.File!.Set(
                        avatar.OriginalFileName,
                        _fileTokenService.CreateToken(userId, avatar.FileId),
                        Hasher.GetKey(avatar.FileId.ToByteArray()))
                });
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<AvatarResponse>.ErrorResult("DataBase error");
        }
    }

    public async Task<Result<AvatarResponse>> GetActualAvatar(GetChatAvatarRequest request, Guid userId)
    {
        try
        {
            ChatAvatar? avatar = await _chatAvatarRepository.GetActualChatAvatarById(request.ChatId);
            Result result = _chatAvatarValidator.IsAvailable(avatar);
            if (!result.Success)
                return Result<AvatarResponse>.From(result);

            return Result<AvatarResponse>.OkResult(
                new()
                {
                    AvatarTokenAccess = _attachmentAccessTokenService.CreateToken(avatar!.Id),
                    FileMetadata = avatar.File!.Set(
                        avatar.OriginalFileName,
                        _fileTokenService.CreateToken(userId, avatar.FileId),
                        Hasher.GetKey(avatar.FileId.ToByteArray()))
                });
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<AvatarResponse>.ErrorResult("DataBase error");
        }
    }

    public async Task<Result<List<AvatarResponse>>> GetAvatars(GetChatAvatarRequest request, Guid userId)
    {
        try
        {
            List<ChatAvatar>? avatars = await _chatAvatarRepository.GetChatAvatarsById(request.ChatId);
            Result result = _chatAvatarValidator.IsAvailableCollection(avatars);
            if (!result.Success)
                return Result<List<AvatarResponse>>.From(result);

            ChatAvatar avatar = null!;
            List<AvatarResponse> response = [];

            for (int i = 0; i < avatars!.Count; i++)
            {
                avatar = avatars[i];
                response.Add(
                    new()
                    {
                        AvatarTokenAccess = _attachmentAccessTokenService.CreateToken(avatar!.Id),
                        FileMetadata = avatar.File!.Set(
                            avatar.OriginalFileName,
                            _fileTokenService.CreateToken(userId, avatar.FileId),
                            Hasher.GetKey(avatar.FileId.ToByteArray()))
                    });
            }

            return Result<List<AvatarResponse>>.OkResult(response);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<List<AvatarResponse>>.ErrorResult("DataBase error");
        }
    }

    public async Task<Result> RemoveAvatar(RemoveChatAvatarRequest request, Guid userId)
    {
        try
        {
            Result ruleResult = await _ruleService.HasPermissionAsync(
                    userId,
                    request.ChatId,
                    Permissions.DeleteAvatar
                );
            if (!ruleResult.Success)
                return ruleResult;

            bool result = await _chatAvatarRepository.TryDeleteAvatarByIds(request.ChatId, request.FileId);

            return result ? Result.OkResult() : Result.BadRequest("Invalid id");
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }

    public async Task<Result> SetAvatar(SetChatAvatarRequest request, Guid userId)
    {
        try
        {
            Result permissionresult = await _ruleService.HasPermissionAsync(userId, request.ChatId, Permissions.ChangeAvatar);
            if (!permissionresult.Success)
                return permissionresult;


            Result<FileMetadatum> result = await _fileMetadatumService.GetByToken(
                request.Metadata.Token,
                userId,
                CommonObjects.DTO.FileCategory.Image);
            if (!result.Success)
                return Result.From(result);

            Result<Chat> chatResult = await _chatService.Get(request.ChatId);
            if (!chatResult.Success)
                return Result.From(chatResult);

            ChatAvatar chatAvatar = new()
            {
                ChatId = request.ChatId,
                FileId = result.Body!.Id,
                OriginalFileName = request.Metadata.OriginalName,
                IsActive = true
            };

            await _chatAvatarRepository.AddAsync(chatAvatar);
            await _chatAvatarPublisherService.Publish(new(
                chatResult.Body!.UniqueName,
                _attachmentAccessTokenService.CreateToken(chatAvatar.Id),
                chatAvatar.FileId,
                chatAvatar.ChatId,
                chatAvatar.OriginalFileName,
                DateTime.UtcNow));
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }
}
