namespace Client.Services.Contract;

public interface ICamera : IDisposable
{
    ReplaySubject<(string, LogLevel)> StatusMsg { get; }
    Subject<Mat> GrabMatSubject { get; }
    ReadOnlyCollection<string> DeviceListStr { get; }
    BehaviorSubject<bool> ConnectStatus { get; }
    BehaviorSubject<bool> IsGrabing { get; }

    Result SearchDevices();

    Result ConnectDevices(int selectIndex = 0);

    Result SetupDevices();

    Result CloseDevices();

    Result StartGrab();

    Result StopGrab();

    Task<Result> FreeGrab(int milsec);
}