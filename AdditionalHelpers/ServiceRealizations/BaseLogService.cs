using AdditionalHelpers.Services;
using System.Runtime.CompilerServices;

namespace AdditionalHelpers.ServiceRealizations;

public abstract class BaseLogService(LogLevel minLogLevel) : ILoggerService
{
    protected readonly LogLevel _minLogLevel = minLogLevel;

    public bool IsEnabled(LogLevel level) =>
        (short)_minLogLevel <= (short)level;

    public static bool CheckFile(LogLevel level) =>
        (short)level <= (short)LogLevel.Warning;

    protected static LogEntry Format(
        LogLevel level,
        string message,
        Exception? exception = null,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "") =>
        new(message, $"File: {callerFile} Method: {caller} Line: {callerLineNumber}", level, DateTime.UtcNow, exception is null ? null : $"\nException: {exception.Message}\n{exception.InnerException}");

    public abstract void Log(LogLevel level,
        string message,
        Exception? exception = null,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "");
    public void Info(string message,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "") =>
        Log(LogLevel.Info, message, caller: caller, callerLineNumber: callerLineNumber, callerFile: callerFile);

    public void Error(string message,
        Exception? exception = null,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "") =>
        Log(LogLevel.Error, message, exception, caller: caller, callerLineNumber: callerLineNumber, callerFile: callerFile);

    public void Fatal(string message,
        Exception? exception = null,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "") =>
        Log(LogLevel.Fatal, message, exception, caller: caller, callerLineNumber: callerLineNumber, callerFile: callerFile);

    public void Debug(string message,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "") =>
        Log(LogLevel.Debug, message, caller: caller, callerLineNumber: callerLineNumber, callerFile: callerFile);

    public void Trace(string message,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "") =>
        Log(LogLevel.Trace, message, caller: caller, callerLineNumber: callerLineNumber, callerFile: callerFile);

    public void Warning(string message,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFile = "") =>
        Log(LogLevel.Warning, message, caller: caller, callerLineNumber: callerLineNumber, callerFile: callerFile);
}

