using DataSaveHelpers.EntityTypesRealizations.Identified;

namespace SpoofSettingsService.Models;

public partial class User : IdentifiedSoftDeletableEntity<Guid>
{
    public DateTime WasOnline { get; set; }

    public string Name { get; set; } = null!;

    public string Login { get; set; } = null!;

    public int MonthsBeforeDelete { get; set; }

    public bool SearchMe { get; set; }

    public bool ShowMe { get; set; }

    public bool ForwardMessage { get; set; }

    public bool InviteMe { get; set; }

    public DateTime LastModified { get; set; }

    public virtual ICollection<ChatUser> ChatUsers { get; set; } = [];

    public virtual ICollection<Chat> Chats { get; set; } = [];

    public virtual ICollection<StickerPack> StickerPacks { get; set; } = [];

    public virtual ICollection<UserAvatar> UserAvatars { get; set; } = [];

    public virtual ICollection<GlobalPermission> GlobalPermissions { get; set; } = [];
}
