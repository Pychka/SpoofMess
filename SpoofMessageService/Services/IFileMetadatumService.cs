using CommonObjects.DTO;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services;

public interface IFileMetadatumService
{
    public Task<Result<FileMetadata>> Get(Guid fileId, Guid userId);
    public Task<Result<FileMetadatum>> Get(Guid fileId);
    public Task<Result> Save(CreateFile createFile);
    public Task<Result> Delete(DeleteFile deleteFile);
}
