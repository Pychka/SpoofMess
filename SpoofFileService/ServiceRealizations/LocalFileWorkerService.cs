using Microsoft.Extensions.Options;
using SpoofFileService.Services;

namespace SpoofFileService.ServiceRealizations;

public class LocalFileWorkerService(IOptions<FileSettings> fileSettings) : IFileWorkerService
{
    private readonly FileSettings _fileSettings = fileSettings.Value;

    public Task<bool> Delete(string filePath)
    {
        if (!File.Exists(
            Path.Combine(
                _fileSettings.StoragePath,
                filePath)))
            return Task.FromResult(false);

        File.Delete(
            Path.Combine(
                _fileSettings.StoragePath,
                filePath));

        return Task.FromResult(true);
    }

    public Task<FileStream?> Get(string filePath)
    {
        if (File.Exists(
            Path.Combine(
                _fileSettings.StoragePath,
                filePath)))
            return Task.FromResult<FileStream?>(
                File.OpenRead(
                    Path.Combine(
                        _fileSettings.StoragePath,
                        filePath)));

        return Task.FromResult<FileStream?>(null);
    }

    public async Task<string> Save(IFormFile file)
    {
        string filePath = GetPath(file.FileName);

        using (FileStream stream = new(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return filePath;
    }

    public async Task Move(string filePath, string newFilePath)
    {
        //May be soon I change it
        File.Move(filePath, GetPath(newFilePath));
    }

    private string GetPath(string path)
    {
        if (!Directory.Exists(_fileSettings.StoragePath))
            Directory.CreateDirectory(_fileSettings.StoragePath);
        return Path.Combine(_fileSettings.StoragePath, $"{Path.GetFileNameWithoutExtension(path)}{Path.GetExtension(path)}");
    }
}
