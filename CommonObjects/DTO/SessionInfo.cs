namespace CommonObjects.DTO;

public class SessionInfo
{
    public Guid Id { get; set; }

    public Guid UserEntryId { get; set; }

    public string DeviceId { get; set; } = null!;

    public string? DeviceName { get; set; }

    public string? Platform { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastActivityAt { get; set; }

    public bool IsActive { get; set; }
}
