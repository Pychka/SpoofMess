using DataHelpers;

namespace SpoofEntranceService.Models;

public partial class UserEntry : IdentifiedEntity<Guid>
{
    public string PasswordHash { get; set; } = null!;

    public string UniqueName { get; set; } = null!;

    public virtual ICollection<SessionInfo> SessionInfos { get; set; } = [];
}
