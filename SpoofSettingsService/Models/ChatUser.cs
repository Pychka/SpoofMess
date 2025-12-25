using System;
using System.Collections.Generic;

namespace SpoofSettingsService.Models;

public partial class ChatUser
{
    public long Id { get; set; }

    public long ChatId { get; set; }

    public long UserId { get; set; }

    public long RoleTypeId { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsMutted { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual RoleType RoleType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
