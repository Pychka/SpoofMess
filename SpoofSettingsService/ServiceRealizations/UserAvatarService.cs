using AdditionalHelpers.Services;
using CommonObjects.Requests.Avatars;
using CommonObjects.Responses;
using CommonObjects.Results;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services;
using SpoofSettingsService.Services.MessageBrokers;
using SpoofSettingsService.Services.Repositories;
using SpoofSettingsService.Services.Validators;
using SpoofSettingsService.Setters;

namespace SpoofSettingsService.ServiceRealizations;

public class UserAvatarService(
        ILoggerService loggerService, 
        IUserAvatarRepository userAvatarRepository, 
        IUserAvatarValidator userAvatarValidator,
        IFileTokenService fileTokenService,
        IUserAvatarPublisherService userAvatarPublisherService,
        IFileMetadatumService fileMetadatumService,
        IUserService userService,
        IAttachmentAccessTokenService attachmentAccessTokenService
    ) : IUserAvatarService
{
    private readonly IUserAvatarPublisherService _userAvatarPublisherService = userAvatarPublisherService;
    private readonly IFileMetadatumService _fileMetadatumService = fileMetadatumService;
    private readonly IUserService _userService = userService;
    private readonly IAttachmentAccessTokenService _attachmentAccessTokenService = attachmentAccessTokenService;
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IUserAvatarRepository _userAvatarRepository = userAvatarRepository;
    private readonly IUserAvatarValidator _userAvatarValidator = userAvatarValidator;

    public async Task<Result<AvatarResponse>> GetAvatar(byte[] accessToken, Guid userId)
    {
        try
        {
            if (!_attachmentAccessTokenService.IsValid(accessToken, out Guid avatarId))
                return Result<AvatarResponse>.Forbidden("Unvalid access token");
                
            UserAvatar? avatar = await _userAvatarRepository.GetByIdAsync(avatarId);
            Result result = _userAvatarValidator.IsAvailable(avatar);
            if (!result.Success)
                return Result<AvatarResponse>.From(result);

            return Result<AvatarResponse>.OkResult(
                new() { 
                    AvatarTokenAccess = _attachmentAccessTokenService.CreateToken(avatar!.Id),
                    FileMetadata = avatar.File!.Set(avatar.OriginalFileName,
                        _fileTokenService.CreateToken(
                            userId,
                            avatar.FileId
                            ),
                        Hasher.GetKey(avatar.FileId.ToByteArray())
                        ) 
                });
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<AvatarResponse>.ErrorResult("DataBase error");
        }
    }

    public async Task<Result<AvatarResponse>> GetActualAvatar(GetUserAvatarRequest request, Guid userId)
    {
        try
        {
            UserAvatar? avatar = await _userAvatarRepository.GetActualUserAvatarById(request.UserId);
            Result result = _userAvatarValidator.IsAvailable(avatar);
            if (!result.Success)
                return Result<AvatarResponse>.From(result);

            return Result<AvatarResponse>.OkResult(
                new()
                {
                    AvatarTokenAccess = _attachmentAccessTokenService.CreateToken(avatar!.Id),
                    FileMetadata = avatar.File!.Set(avatar.OriginalFileName,
                        _fileTokenService.CreateToken(
                            userId,
                            avatar.FileId
                            ),
                        Hasher.GetKey(avatar.FileId.ToByteArray())
                        )
                });
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<AvatarResponse>.ErrorResult("DataBase error");
        }
    }

    public async Task<Result<List<AvatarResponse>>> GetAvatars(GetUserAvatarRequest request, Guid userId)
    {
        try
        {
            List<UserAvatar>? avatars = await _userAvatarRepository.GetUserAvatarsById(request.UserId);
            Result result = _userAvatarValidator.IsAvailableCollection(avatars);
            if (!result.Success)
                return Result<List<AvatarResponse>>.From(result);

            UserAvatar avatar = null!;
            Result<List<AvatarResponse>> response = Result<List<AvatarResponse>>.OkResult([]);

            for (int i = 0; i < avatars!.Count; i++)
            {
                avatar = avatars[i];
                response.Body!.Add(new() 
                { 
                    AvatarTokenAccess = _attachmentAccessTokenService.CreateToken(avatar.Id),
                    FileMetadata = avatar.File!.Set(
                        avatar.OriginalFileName,
                        _fileTokenService.CreateToken(
                            userId,
                            avatar.FileId
                            ),
                        Hasher.GetKey(avatar.FileId.ToByteArray()))
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<List<AvatarResponse>>.ErrorResult("DataBase error");
        }
    }
    public async Task<Result> RemoveAvatar(RemoveUserAvatarRequest request, Guid userId)
    {
        try
        {
            bool result = await _userAvatarRepository.TryDeleteAvatarByIds(userId, request.FileId);

            return result ? Result.OkResult() : Result.BadRequest("Invalid id");
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("DataBase error");
        }
    }

    public async Task<Result> SetAvatar(SesUserAvatarRequest request, Guid userId)
    {
        try
        {
            Result<User> userResult = await _userService.Get(userId);
            if (!userResult.Success)
                return Result.From(userResult);

            Result<FileMetadatum> result = await _fileMetadatumService.GetByToken(
                request.Metadata.Token,
                userId,
                CommonObjects.DTO.FileCategory.Image);
            if (!result.Success)
                return Result.From(result);

            UserAvatar avatar = new()
            {
                UserId = userId,
                FileId = result.Body!.Id,
                OriginalFileName = request.Metadata.OriginalName,
                IsActive = true
            };

            await _userAvatarRepository.AddAsync(avatar);
            await _userAvatarPublisherService.Publish(new(
                userResult.Body!.Login,
                _attachmentAccessTokenService.CreateToken(avatar.Id),
                avatar.FileId,
                avatar.UserId,
                avatar.OriginalFileName,
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
