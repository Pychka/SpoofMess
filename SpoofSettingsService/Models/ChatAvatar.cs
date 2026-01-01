using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class ChatAvatar : IdentifiedEntity<long>, IChangeable
{
    public long? ChatId { get; set; }

    public long? FileId { get; set; }

    public bool IsActive { get; set; }

    public DateTime LastModified { get; set; }

    public virtual Chat? Chat { get; set; }

    public virtual FileMetadatum? File { get; set; }
}
