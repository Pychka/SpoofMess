using CommonObjects.Results;
using SpoofSettingsService.Models;
using SpoofSettingsService.ServiceRealizations.Validators;

namespace SpoofSettingsServiceTests.ValidatorsTests;

public class StickerValidatorTests
{
    [Theory]
    [MemberData(nameof(GetStickers))]
    public void Validate_sticker_to_null_or_deleted_and_null_file(Sticker entity, int statusCode)
    {
        StickerValidator sut = new();

        Result result = sut.IsAvailable(entity);

        Assert.Equal(statusCode, result.StatusCode);
    }

    public static TheoryData<Sticker, int> GetStickers()
    {
        TheoryData<Sticker, int> data = [];
        data.Add(null!, 404);
        data.Add(new Sticker { IsDeleted = true, FileId = Guid.Empty }, 400);
        data.Add(new Sticker { IsDeleted = false, FileId = Guid.Empty }, 200);
        return data;
    }
}
