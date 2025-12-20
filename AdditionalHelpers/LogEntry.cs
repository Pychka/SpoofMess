namespace AdditionalHelpers;

public class LogEntry
{
    public string Message { get; set; } = null!;

    public LogLevel Level { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
}
