using DataHelpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpoofEntranceService.Models;

public partial class Token : IdentifiedEntity<string>, ISoftDeletable
{
    [Column("RefreshTokenHash")]
    public new string Id { get; set; } = default!;

    public Guid SessionInfoId { get; set; }

    public DateTime ValidTo { get; set; }

    public virtual SessionInfo SessionInfo { get; set; } = null!;

    public Token() { }

    public Token(string refreshTokenHash, Guid sessionInfoId, DateTime validTo)
    {
        Id = refreshTokenHash;
        SessionInfoId = sessionInfoId;
        ValidTo = validTo;
    }
}
