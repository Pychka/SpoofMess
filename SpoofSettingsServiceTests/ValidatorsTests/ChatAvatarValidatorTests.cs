using CommonObjects.Results;
using SpoofSettingsService.Models;
using SpoofSettingsService.ServiceRealizations.Validators;

namespace SpoofSettingsServiceTests.ValidatorsTests;

public class ChatAvatarValidatorTests
{
    [Theory]
    [MemberData(nameof(GetAvatars), DisableDiscoveryEnumeration = true)]
    public void Validate_awatars_collection(List<ChatAvatar> entities, int statusCode)
    {
        ChatAvatarValidator sut = new();

        Result result = sut.IsAvailableCollection(entities);

        Assert.Equal(statusCode, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAvatarWithFile), DisableDiscoveryEnumeration = true)]
    public void Validate_file_to_null_deleted_and_file(ChatAvatar entity, int statusCode)
    {
        ChatAvatarValidator sut = new();

        Result result = sut.FileIsActive(entity);

        Assert.Equal(statusCode, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAvatar), DisableDiscoveryEnumeration = true)]
    public void Validate_file_to_null_deleted(ChatAvatar entity, int statusCode)
    {
        ChatAvatarValidator sut = new();

        Result result = sut.IsAvailable(entity);

        Assert.Equal(statusCode, result.StatusCode);
    }

    public static TheoryData<List<ChatAvatar>, int> GetAvatars()
    {
        TheoryData<List<ChatAvatar>, int> data = [];
        data.Add(null!, 404 );
        data.Add([], 400);
        data.Add([new()], 200);
        data.Add([ new(), new(), new() ], 200);
        return data;
    }
    public static TheoryData<ChatAvatar, int> GetAvatarWithFile()
    {
        TheoryData<ChatAvatar, int> data = [];
        data.Add(null!, 404);
        data.Add(new() { IsDeleted = true }, 400 );
        data.Add(new() { IsDeleted = true, FileId = Guid.Empty }, 400 );
        data.Add(new() { IsDeleted = false }, 400 );
        data.Add(new() { IsDeleted = false, FileId = Guid.Empty }, 400 );
        data.Add(new() { IsDeleted = false, File = new() }, 200 );
        data.Add(new() { IsDeleted = false, File = new(), FileId = Guid.Empty }, 200 );
        return data;
    }
    public static TheoryData<ChatAvatar, int> GetAvatar()
    {
        TheoryData<ChatAvatar, int> data = [];
        data.Add(null!, 404);
        data.Add(new() { IsDeleted = true }, 400);
        data.Add(new() { IsDeleted = false }, 200);
        return data;
    }
}
