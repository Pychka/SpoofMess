
namespace AdditionalHelpers;

public class ConsoleLoggerService(LogLevel minLogLevel) : ILoggerService
{
    private readonly LogLevel _minLogLevel = minLogLevel;

    private readonly List<LogColor> colors = [
            new(LogLevel.Fatal, ConsoleColor.DarkYellow),
            new(LogLevel.Critical, ConsoleColor.Red),
            new(LogLevel.Error, ConsoleColor.DarkRed),
            new(LogLevel.Warning, ConsoleColor.Yellow),
            new(LogLevel.Info, ConsoleColor.Green),
            new(LogLevel.Debug, ConsoleColor.DarkGray),
            new(LogLevel.Trace, ConsoleColor.Gray),
        ];
    public bool IsEnabled(LogLevel level) => _minLogLevel >= level;

    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        ConsoleColor lastColor = Console.ForegroundColor;
        Console.ForegroundColor = colors.FirstOrDefault(x => x.LogLevel == level)?.Color ?? ConsoleColor.Blue;
        Console.Write(level);
        Console.ForegroundColor = lastColor;
        Console.WriteLine($": {message}{((int)_minLogLevel < 2 ? (exception is null ? "null exception" : $"{exception.Message}\n{exception.InnerException}") : "")}");
    }
}
