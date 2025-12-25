using System;
using System.Collections.Generic;

namespace SpoofSettingsService.Models;

public partial class UserAvatar
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public long? FileId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime LastModified { get; set; }

    public virtual FileMetadatum? File { get; set; }

    public virtual User? User { get; set; }
}
