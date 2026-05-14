using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CommonObjects.DTO;

public record SearchableEntity(
    Guid Id,
    string? Name,
                               SearchType Type,
                               string UniqueName,
                               string? OriginalFileName)
{
    [NotMapped]
    public byte[]? AvatarToken { get; set; }
    [NotMapped]
    public byte[]? FileId { get; set; }

    [JsonIgnore]
    public Guid? AvatarId { get; set; }
}