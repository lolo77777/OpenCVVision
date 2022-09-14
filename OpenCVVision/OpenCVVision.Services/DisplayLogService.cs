using DynamicData;

using OpenCVVision.Contracts;
using OpenCVVision.Model.Entity;

namespace OpenCVVision.Services;

public class DisplayLogService : IDisplayLog
{
    private int _id;

    public SourceCache<LogData, int> LogCache { get; } = new SourceCache<LogData, int>(t => t.Id);

    public void Clear()
    {
        _id = 0;
        LogCache.Clear();
    }

    public void Log(string msg, DateTime dateTime, LogLevel logLevel)
    {
        if (_id == 100)
        {
            Clear();
        }
        var logdata = new LogData(_id, msg, dateTime, logLevel);
        LogCache.AddOrUpdate(logdata);

        _id++;
    }
}