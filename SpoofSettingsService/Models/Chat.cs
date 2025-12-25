namespace SpoofSettingsService.Models;

public partial class Chat
{
    public long Id { get; set; }

    public long ChatTypeId { get; set; }

    public long? OwnerId { get; set; }

    public string? ChatName { get; set; }

    public bool? IsPublic { get; set; }

    public string? UniqueName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastModified { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ChatAvatar> ChatAvatars { get; set; } = new List<ChatAvatar>();

    public virtual ChatType ChatType { get; set; } = null!;

    public virtual ICollection<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();

    public virtual User? Owner { get; set; }
}
