using CommonObjects.Results;
using SpoofSettingsService.Models;
using SpoofSettingsService.ServiceRealizations.Validators;

namespace SpoofSettingsServiceTests.ValidatorsTests;

public class UserAvatarValidatorTests
{
    [Theory]
    [MemberData(nameof(GetAvatars))]
    public void Validate_awatars_collection(List<UserAvatar> entities, int statusCode)
    {
        UserAvatarValidator sut = new();

        Result result = sut.IsAvailableCollection(entities);

        Assert.Equal(statusCode, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAvatarWithFile))]
    public void Validate_file_to_null_deleted_and_file(UserAvatar entity, int statusCode)
    {
        UserAvatarValidator sut = new();

        Result result = sut.FileIsActive(entity);

        Assert.Equal(statusCode, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAvatar))]
    public void Validate_file_to_null_deleted(UserAvatar entity, int statusCode)
    {
        UserAvatarValidator sut = new();

        Result result = sut.IsAvailable(entity);

        Assert.Equal(statusCode, result.StatusCode);
    }

    public static TheoryData<List<UserAvatar>, int> GetAvatars()
    {
        TheoryData<List<UserAvatar>, int> data = [];
        data.Add(null!, 404);
        data.Add([], 400);
        data.Add([new()], 200);
        data.Add([new(), new(), new(), new()], 200);
        return data;
    }
    public static TheoryData<UserAvatar, int> GetAvatarWithFile()
    {
        TheoryData<UserAvatar, int> data = [];
        data.Add(null!, 404);
        data.Add(new UserAvatar { IsDeleted = true, FileId = Guid.Empty }, 400);
        data.Add(new UserAvatar { IsDeleted = false, FileId = Guid.Empty }, 200);
        return data;
    }
    public static TheoryData<UserAvatar, int> GetAvatar()
    {
        TheoryData<UserAvatar, int> data = [];
        data.Add(null!, 404 );
        data.Add(new UserAvatar { IsDeleted = true }, 400 );
        data.Add(new UserAvatar { IsDeleted = false }, 200 );
        return data;
    }
}
