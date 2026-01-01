using DataHelpers;

namespace SpoofSettingsService.Models;

public partial class ChatType : IdentifiedEntity<long>
{
    public string Title { get; set; } = null!;

    public virtual ICollection<Chat> Chats { get; set; } = [];
}
