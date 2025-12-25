using System;
using System.Collections.Generic;

namespace SpoofSettingsService.Models;

public partial class Sticker
{
    public long Id { get; set; }

    public long StickerPackId { get; set; }

    public long? FileId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime LastModified { get; set; }

    public bool IsDeleted { get; set; }

    public virtual FileMetadatum? File { get; set; }

    public virtual StickerPack StickerPack { get; set; } = null!;
}
