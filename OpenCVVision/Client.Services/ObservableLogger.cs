using Client.Contracts;

using NLog;

using System.ComponentModel;

using LogLevel = Splat.LogLevel;

namespace Client.Services;

public class ObservableLogger : Splat.ILogger
{
    private readonly Logger? _logger;
    private readonly IDisplayLog? _displayLog;
    public static Subject<(string msg, LogLevel level)> LogMsg { get; } = new Subject<(string msg, LogLevel level)>();

    public ObservableLogger()
    {
        _logger = Locator.Current.GetService<Logger>();
        _displayLog = Locator.Current.GetService<IDisplayLog>();
        Task.Run(async () => await ClearLogs());
    }

    public LogLevel Level { get; }

    public void Write(string message, LogLevel logLevel)
    {
        //WriteDebug(message, logLevel);
        WriteTxt(message, logLevel);

        if (logLevel > LogLevel.Debug && !message.Contains("POCO") && !message.Contains("MessageBus"))
        {
            _displayLog?.Log(message, DateTime.Now, logLevel);
            LogMsg.OnNext((message, logLevel));
        }
    }

    public void Write(Exception exception, [Localizable(false)] string message, LogLevel logLevel)
    {
        WriteTxt(message, logLevel);

        if (logLevel > LogLevel.Debug && !message.Contains("POCO") && !message.Contains("MessageBus"))
        {
            _displayLog?.Log(message, DateTime.Now, logLevel);
            LogMsg.OnNext((message, logLevel));
        }
    }

    public void Write([Localizable(false)] string message, [Localizable(false)] Type type, LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    public void Write(Exception exception, [Localizable(false)] string message, [Localizable(false)] Type type, LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    private static async Task ClearLogs()
    {
        await Task.Run(() =>
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
            {
                var path = Path.Combine(Environment.CurrentDirectory, "logs");
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    var time = File.GetCreationTime(file);
                    if (DateTime.Now.Subtract(time).TotalDays > 14)
                    {
                        File.Delete(file);
                    }
                }
            }
        });
    }

    private void WriteTxt(string message, LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Debug:
                _logger?.Debug(message);
                break;

            case LogLevel.Info:
                _logger?.Info(message);
                break;

            case LogLevel.Warn:
                _logger?.Warn(message);
                break;

            case LogLevel.Error:
                _logger?.Error(message);

                break;

            case LogLevel.Fatal:
                _logger?.Fatal(message);

                break;

            default:
                break;
        }
    }
}