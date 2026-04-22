using DataSaveHelpers.EntityTypesRealizations.Identified;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpoofMessageService.Models;

public partial class Attachment : IdentifiedSoftDeletableChangeableEntity<Guid>
{
    public Guid MessageId { get; set; }

    public Guid FileMetadataId { get; set; }

    public string OriginalFileName { get; set; } = null!;

    public virtual FileMetadatum FileMetadata { get; set; } = null!;

    public virtual Message Message { get; set; } = null!;

    [NotMapped]
    public long Size { get; set; }
    [NotMapped]
    public string Category { get; set;  } = string.Empty;
    [NotMapped]
    public string? AdditionalMetadata { get; set; }
}
