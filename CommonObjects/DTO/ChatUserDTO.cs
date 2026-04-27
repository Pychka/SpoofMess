using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonObjects.DTO;

public class ChatUserDTO
{
    public Guid Id { get; set; }

    public int ChatTypeId { get; set; }

    public string UniqueName { get; set; } = string.Empty;

    [JsonIgnore]
    public Guid? FileId { get; set; }

    public string? OriginalFileName { get; set; }

    [JsonIgnore]
    public Guid? AvatarId { get; set; }

    [NotMapped]
    public byte[]? ChatAvatarToken { get; set; }

    [NotMapped]
    public byte[]? ChatAvatarId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Metadata { get; set; }

    public string Name { get; set; } = string.Empty;

    [NotMapped]
    [JsonIgnore]
    public List<PermissionResult> Rules =>
            string.IsNullOrEmpty(RulesJson) ? [] : JsonSerializer.Deserialize<List<PermissionResult>>(RulesJson) ?? [];

    public string RulesJson { get; set; }

}