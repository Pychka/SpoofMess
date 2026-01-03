using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class ChatUser : IdentifiedEntity<Guid>
{
    public Guid ChatId { get; set; }

    public Guid UserId { get; set; }

    public long RoleTypeId { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsMutted { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual RoleType RoleType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
