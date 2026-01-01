using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class Chat : IdentifiedEntity<long>, IChangeable
{
    public long ChatTypeId { get; set; }

    public long? OwnerId { get; set; }

    public string? ChatName { get; set; }

    public bool? IsPublic { get; set; }

    public string? UniqueName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastModified { get; set; }

    public virtual ICollection<ChatAvatar> ChatAvatars { get; set; } = new List<ChatAvatar>();

    public virtual ChatType ChatType { get; set; } = null!;

    public virtual ICollection<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();

    public virtual User? Owner { get; set; }

    public Chat() { }

    public Chat(long chatTypeId, long? ownerId, string? chatName, bool? isPublic, string? uniqueName, DateTime createdAt, DateTime lastModified)
    {
        ChatTypeId = chatTypeId;
        OwnerId = ownerId;
        ChatName = chatName;
        IsPublic = isPublic;
        UniqueName = uniqueName;
        CreatedAt = createdAt;
        LastModified = lastModified;
    }
}
