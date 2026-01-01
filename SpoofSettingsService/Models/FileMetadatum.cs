using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class FileMetadatum : IdentifiedEntity<long>
{
    public string Bucket { get; set; } = null!;

    public string ObjectKey { get; set; } = null!;

    public int ExtensionId { get; set; }

    public virtual ICollection<ChatAvatar> ChatAvatars { get; set; } = [];

    public virtual Extension Extension { get; set; } = null!;

    public virtual ICollection<StickerPack> StickerPacks { get; set; } = [];

    public virtual ICollection<Sticker> Stickers { get; set; } = [];

    public virtual ICollection<UserAvatar> UserAvatars { get; set; } = [];
}
