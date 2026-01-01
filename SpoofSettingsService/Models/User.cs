using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class User : IdentifiedEntity<long>
{
    public DateTime WasOnline { get; set; }

    public string Name { get; set; } = null!;

    public long MonthsBeforeDelete { get; set; }

    public bool SearchMe { get; set; }

    public bool ShowMe { get; set; }

    public bool ForwardMessage { get; set; }

    public bool InviteMe { get; set; }

    public virtual ICollection<ChatUser> ChatUsers { get; set; } = [];

    public virtual ICollection<Chat> Chats { get; set; } = [];

    public virtual ICollection<StickerPack> StickerPacks { get; set; } = [];

    public virtual ICollection<UserAvatar> UserAvatars { get; set; } = [];
}
