namespace AdditionalHelpers;

public interface ILoggerService
{
    void Info(string message);
    void Error(string message, Exception? exception = null);
    void Fatal(string message, Exception? exception = null);
    void Debug(string message);
    void Trace(string message);
    void Warning(string message);
    bool IsEnabled(LogLevel level);
}
