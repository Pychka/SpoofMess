using System;
using System.Collections.Generic;

namespace SpoofSettingsService.Models;

public partial class ChatType
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
}
