using System.Text;

namespace AdditionalHelpers;

public class FileLoggerService(LogLevel minLevel, string directoryPath, long maxSize = 1024 * 50, int maxFiles = 10, int bufferSize = 1024 * 4) : ILoggerService
{
    private readonly LogLevel _minLevel = minLevel;
    
    private readonly long _maxSize = maxSize;
    private readonly int _maxFiles = maxFiles;
    private readonly int _bufferSize = bufferSize;

    private readonly Lock _lock = new();
    private readonly string _directoryPath = directoryPath;
    
    private string? _currentDirectoryPath;
    private string? currentFile;
    
    private StreamWriter? _writer;
    
    private int _bytesInBuffer = 0;
    private const long FLUSH_THRESHOLD = 65536;

    private static string ObjectName => DateTime.UtcNow.ToString("dd.M.yyyy HH:mm:ss:fffffff").Replace(':', '-').Replace(' ', '_') + ".txt";

    public bool IsEnabled(LogLevel level) => _minLevel >= level;

    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        if (!IsEnabled(level))
            return;

        LogEntry logEntry = new()
        {
            Message = message,
            Level = level,
        };

        WriteToFile(logEntry);
    }

    private void WriteToFile(LogEntry logEntry)
    {
        if (!Directory.Exists(_directoryPath))
            Directory.CreateDirectory(_directoryPath);

        if (currentFile is null || new FileInfo(currentFile).Length >= _maxSize)
        {
            if (_currentDirectoryPath is null || new DirectoryInfo(_currentDirectoryPath).EnumerateFiles().Count() > _maxFiles)
                _currentDirectoryPath = Path.Combine(_directoryPath, ObjectName + '\\');

            currentFile = Path.Combine(_currentDirectoryPath, ObjectName);
            OpenOrCreateFile();
        }


        lock (_lock)
        {
            try
            {
                if (_writer == null) return;

                _writer.WriteLine(logEntry);
                _bytesInBuffer += Encoding.UTF8.GetByteCount(logEntry + Environment.NewLine);

                if (_bytesInBuffer >= FLUSH_THRESHOLD)
                {
                    ForceFlush();
                }
            }
            catch
            {
            }
        }
    }

    private void ForceFlush()
    {
        lock (_lock)
        {
            try
            {
                _writer?.Flush();
                _bytesInBuffer = 0;
            }
            catch
            {
            }
        }
    }

    private void OpenOrCreateFile()
    {
        _writer?.Dispose();

        _writer =  new(currentFile!, true, Encoding.UTF8, _bufferSize)
        {
            AutoFlush = false,
            NewLine = Environment.NewLine
        };

        _bytesInBuffer = 0;
    }

    public void Info(string message) =>
        Log(LogLevel.Info, message);

    public void Error(string message, Exception? exception = null) =>
        Log(LogLevel.Error, message, exception);

    public void Fatal(string message, Exception? exception = null) =>
        Log(LogLevel.Fatal, message, exception);

    public void Debug(string message) =>
        Log(LogLevel.Debug, message);

    public void Trace(string message) =>
        Log(LogLevel.Trace, message);

    public void Warning(string message) =>
        Log(LogLevel.Warning, message);
}
