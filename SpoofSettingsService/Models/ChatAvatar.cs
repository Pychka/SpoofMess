namespace SpoofSettingsService.Models;

public partial class ChatAvatar
{
    public long Id { get; set; }

    public long? ChatId { get; set; }

    public long? FileId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime LastModified { get; set; }

    public virtual Chat? Chat { get; set; }

    public virtual FileMetadatum? File { get; set; }
}
