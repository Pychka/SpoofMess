using DataSaveHelpers.EntityTypesRealizations.Identified;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpoofSettingsService.Models;

public partial class Chat : IdentifiedSoftDeletableChangeableEntity<Guid>
{
    public int ChatTypeId { get; set; }

    public Guid? OwnerId { get; set; }

    public string UniqueName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public long? BaseRoleId { get; set; }

    public virtual ChatRole? BaseRole { get; set; }

    [NotMapped]
    public ChatAvatar? ActualAvatar => ChatAvatars.FirstOrDefault(x => x.IsActive);

    public virtual ICollection<ChatAvatar> ChatAvatars { get; set; } = [];

    public virtual ICollection<ChatRole> ChatRoles { get; set; } = [];

    public virtual ChatType ChatType { get; set; } = null!;

    public virtual ICollection<ChatUser> ChatUsers { get; set; } = [];

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<RoleRank> RoleRanks { get; set; } = [];

    public Chat() { }

    public Chat(Guid id, int chatTypeId, Guid? ownerId, string chatName, string chatUniqueName, DateTime createdAt, DateTime lastModified)
    {
        Id = id;
        ChatTypeId = chatTypeId;
        OwnerId = ownerId;
        Name = chatName;
        UniqueName = chatUniqueName;
        CreatedAt = createdAt;
        LastModified = lastModified;
    }
}
