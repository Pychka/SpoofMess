using DataHelpers;

namespace SpoofEntranceService.Models;

public partial class SessionInfo : IdentifiedEntity<Guid>, IIdentifiedEntity
{
    public Guid UserEntryId { get; set; }

    public string DeviceId { get; set; } = null!;

    public string? DeviceName { get; set; }

    public string? Platform { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastActivityAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = [];

    public virtual UserEntry UserEntry { get; set; } = null!;
}
