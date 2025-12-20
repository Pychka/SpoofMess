namespace SpoofEntranceService.Models;

public partial class Token
{
    public string RefreshTokenHash { get; set; } = null!;

    public Guid SessionInfoId { get; set; }

    public DateTime ValidTo { get; set; }

    public bool IsDeleted { get; set; }

    public virtual SessionInfo SessionInfo { get; set; } = null!;
}
