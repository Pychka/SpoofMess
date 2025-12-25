namespace SpoofSettingsService.Models;

public partial class RoleType
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public bool SendMessage { get; set; }

    public bool ReceiveMessage { get; set; }

    public bool ManageChat { get; set; }

    public bool ForwardMessage { get; set; }

    public virtual ICollection<ChatUser> ChatUsers { get; set; } = [];
}
