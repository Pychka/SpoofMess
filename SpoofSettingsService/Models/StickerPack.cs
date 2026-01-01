using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class StickerPack : IdentifiedEntity<long>, IChangeable
{
    public long? AuthorId { get; set; }

    public string Title { get; set; } = null!;

    public long? PreviewId { get; set; }

    public DateTime LastModified { get; set; }

    public virtual User? Author { get; set; }

    public virtual FileMetadatum? Preview { get; set; }

    public virtual ICollection<Sticker> Stickers { get; set; } = [];
}
