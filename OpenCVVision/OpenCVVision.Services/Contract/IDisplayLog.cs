using DynamicData;

using OpenCVVision.Model.Entity;

namespace OpenCVVision.Contracts;

public interface IDisplayLog
{
    public SourceCache<LogData, int> LogCache { get; }

    void Log(string msg, DateTime dateTime, LogLevel logLevel);

    void Clear();
}