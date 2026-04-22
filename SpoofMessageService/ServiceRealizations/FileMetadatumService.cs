using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SecurityLibrary.Tokens;
using SpoofMessageService.Models;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Repositories;
using SpoofMessageService.Services.Validators;

namespace SpoofMessageService.ServiceRealizations;

public class FileMetadatumService(
    IFileMetadatumRepository fileMetadatumRepository,
    ILoggerService loggerService, 
    IUserService userService,
    IFileTokenService fileTokenService,
    IFileMetadatumValidator fileMetadatumValidator) : IFileMetadatumService
{
    private readonly IUserService _userService = userService;
    private readonly IFileMetadatumRepository _fileMetadatumRepository = fileMetadatumRepository;
    private readonly IFileMetadatumValidator _fileMetadatumValidator = fileMetadatumValidator;
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly ILoggerService _loggerService = loggerService;
    public async Task<Result<FileMetadata>> Get(Guid fileId, Guid userId)
    {
        try
        {
            Result<User> user = await _userService.Get(userId);
            if (!user.Success)
                return Result<FileMetadata>.From(user);
            FileMetadatum? fileMetadatum = await _fileMetadatumRepository.GetByIdAsync(fileId);
            Result result = _fileMetadatumValidator.IsAvailable(fileMetadatum);
            if (!result.Success)
                return Result<FileMetadata>.From(result);
            return Result<FileMetadata>.OkResult(new(_fileTokenService.CreateToken(userId, fileMetadatum!.Id), fileMetadatum!.Id.ToByteArray(), string.Empty, fileMetadatum.Size));
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<FileMetadata>.InternalServerError();
        }
    }

    public async Task<Result<FileMetadatum>> Get(Guid fileId)
    {
        try
        {
            FileMetadatum? fileMetadatum = await _fileMetadatumRepository.GetByIdAsync(fileId);
            Result result = _fileMetadatumValidator.IsAvailable(fileMetadatum);
            if (!result.Success)
                return Result<FileMetadatum>.From(result);
            return Result<FileMetadatum>.OkResult(fileMetadatum!);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<FileMetadatum>.InternalServerError();
        }
    }

    public async Task<Result> Save(CreateFile createFile)
    {
        try
        {
            FileMetadatum? fileMetadatum = new()
            {
                Id = createFile.FileId,
                Size = createFile.Size,
                Category = createFile.Category,
                Metadata = createFile.Metadata,
            };
            await _fileMetadatumRepository.AddAsync(fileMetadatum);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.InternalServerError();
        }
    }

    public async Task<Result> Delete(DeleteFile deleteFile)
    {
        try
        {
            return await _fileMetadatumRepository.SoftDeleteAsync(deleteFile.FileId) 
                ? Result.OkResult() 
                : Result.BadRequest("Invalid id");
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.InternalServerError();
        }
    }
}
