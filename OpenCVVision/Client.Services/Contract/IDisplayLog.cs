using Client.Model.Entity;

using DynamicData;

namespace Client.Contracts;

public interface IDisplayLog
{
    public SourceCache<LogData, int> LogCache { get; }

    void Log(string msg, DateTime dateTime, LogLevel logLevel);

    void Clear();
}