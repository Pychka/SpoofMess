namespace AdditionalHelpers;

public interface ILoggerService
{
    void Log(LogLevel level, string message, Exception? exception = null);
    bool IsEnabled(LogLevel level);
}
