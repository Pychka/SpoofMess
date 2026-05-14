using AdditionalHelpers.Services;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace AdditionalHelpers.ServiceRealizations;

public class ConsoleLoggerService : BaseLogService, ILoggerService, IDisposable
{
    private readonly BlockingCollection<LogEntry> entries = [];
    private readonly CancellationTokenSource tokenSource = new();
    private bool canWrite = true;

    private readonly List<LogColor> colors = [
            new(LogLevel.Fatal, ConsoleColor.DarkYellow),
            new(LogLevel.Critical, ConsoleColor.Red),
            new(LogLevel.Error, ConsoleColor.DarkRed),
            new(LogLevel.Warning, ConsoleColor.Yellow),
            new(LogLevel.Info, ConsoleColor.Green),
            new(LogLevel.Debug, ConsoleColor.DarkGray),
            new(LogLevel.Trace, ConsoleColor.Gray),
        ];

    public ConsoleLoggerService(LogLevel minLogLevel) : base(minLogLevel)
    {
        Task.Run(Consume, tokenSource.Token);
    }

    public override void Log(LogLevel level, string message, Exception? exception = null, [CallerMemberName] string caller = "", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFile = "")
    {
        if (!canWrite)
            return;
        LogEntry logEntry = Format(level, message, exception, caller, callerLineNumber, callerFile);
        entries.Add(logEntry);
    }

    private static void ColorPrint(string message, ConsoleColor color, bool newLine = false)
    {
        ConsoleColor lastColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        if (newLine)
            Console.WriteLine(message);
        else
            Console.Write(message);
        Console.ForegroundColor = lastColor;
    }

    private void Consume()
    {
        ConsoleColor color;
        foreach (LogEntry logEntry in entries.GetConsumingEnumerable())
        {
            color = colors.FirstOrDefault(x => x.LogLevel == logEntry.Level)?.Color ?? ConsoleColor.Blue;
            ColorPrint(CheckFile(LogLevel.Debug) ? logEntry.Caller : "", color, true);
            ColorPrint(logEntry.PrintInfo(), color, false);
            Console.WriteLine(logEntry.PrintMessage((int)_minLogLevel < 2));
        }
    }

    public void Dispose()
    {
        canWrite = false;
        while (entries.Count > 0) ;

        tokenSource.Cancel();
        GC.SuppressFinalize(this);
    }
}
