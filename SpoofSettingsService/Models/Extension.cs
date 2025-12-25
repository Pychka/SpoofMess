namespace SpoofSettingsService.Models;

public partial class Extension
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public byte FileCategory { get; set; }

    public virtual ICollection<FileMetadatum> FileMetadata { get; set; } = new List<FileMetadatum>();
}
