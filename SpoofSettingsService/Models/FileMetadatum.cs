namespace SpoofSettingsService.Models;

public partial class FileMetadatum
{
    public long Id { get; set; }

    public string Bucket { get; set; } = null!;

    public string ObjectKey { get; set; } = null!;

    public int ExtensionId { get; set; }

    public virtual ICollection<ChatAvatar> ChatAvatars { get; set; } = new List<ChatAvatar>();

    public virtual Extension Extension { get; set; } = null!;

    public virtual ICollection<StickerPack> StickerPacks { get; set; } = new List<StickerPack>();

    public virtual ICollection<Sticker> Stickers { get; set; } = new List<Sticker>();

    public virtual ICollection<UserAvatar> UserAvatars { get; set; } = new List<UserAvatar>();
}
