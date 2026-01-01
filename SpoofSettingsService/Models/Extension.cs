using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class Extension : IdentifiedEntity<long>
{
    public string Title { get; set; } = null!;

    public byte FileCategory { get; set; }

    public virtual ICollection<FileMetadatum> FileMetadata { get; set; } = [];
}
