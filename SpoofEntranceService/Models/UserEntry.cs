namespace SpoofEntranceService.Models;

public partial class UserEntry
{
    public Guid Id { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string UniqueName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<SessionInfo> SessionInfos { get; set; } = [];
}
