using DataSaveHelpers.EntityTypesRealizations.Identified;

namespace SpoofMessageService.Models;

public partial class Chat : IdentifiedSoftDeletableChangeableEntity<Guid>
{
    public Guid? AvatarId { get; set; }

    public string UniqueName { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public string? Name { get; set; }

    public bool IsPublic { get; set; }

    public virtual FileMetadatum? Avatar { get; set; }

    public virtual ICollection<ChatUser> ChatUsers { get; set; } = [];

    public virtual ICollection<Message> Messages { get; set; } = [];
}
