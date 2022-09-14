using Splat;

namespace OpenCVVision.Model.Entity;

public record LogData
{
    public int Id { get; set; }
    public string Msg { get; set; }
    public DateTime Time { get; set; }
    public LogLevel LogLevel { get; set; }
    public LogData(int id, string msg, DateTime dateTime, LogLevel logLevel)
    {
        Id = id;
        Msg = msg;
        Time = dateTime;
        LogLevel = logLevel;
    }
}