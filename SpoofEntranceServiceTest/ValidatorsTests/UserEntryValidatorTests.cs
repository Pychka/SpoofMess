using CommonObjects.Results;
using SpoofEntranceService.Models;
using SpoofEntranceService.ServiceRealizations.Validators;

namespace SpoofEntranceServiceTest.ValidatorsTests;

public class UserEntryValidatorTests
{
    [Theory]
    [MemberData(nameof(GetUserEntries))]
    public void Check_validate_user_entry(UserEntry user, int statusCode)
    {
        UserEntryValidator sut = new();

        Result result = sut.IsAvailable(user);

        Assert.Equal(statusCode, result.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetActiveUserEntries))]
    public void Check_active_user_entry(UserEntry user, int statusCode)
    {
        UserEntryValidator sut = new();

        Result result = sut.HisIsActive(user);

        Assert.Equal(statusCode, result.StatusCode);
    }

    public static TheoryData<UserEntry, int> GetUserEntries()
    {
        TheoryData<UserEntry, int> data = [];
        data.Add(null!, 404);
        data.Add(new UserEntry { IsDeleted = true }, 400);
        data.Add(new UserEntry { IsDeleted = false }, 200);
        return data;
    }

    public static TheoryData<UserEntry, int> GetActiveUserEntries()
    {
        TheoryData<UserEntry, int> data = [];
        data.Add(null!, 200);
        data.Add(new UserEntry { IsDeleted = true }, 200);
        data.Add(new UserEntry { IsDeleted = false }, 400);
        return data;
    }
}
