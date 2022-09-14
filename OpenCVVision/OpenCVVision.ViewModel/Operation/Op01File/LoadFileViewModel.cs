namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(1, "加载图片", "File")]
public class LoadFileViewModel : OperaViewModelBase
{
    private readonly Interaction<Unit, string> _loadFileConfirm = new();
    public Interaction<Unit, string> LoadFileConfirm => _loadFileConfirm;
    public ReactiveCommand<Unit, Unit> LoadImageCommand { get; private set; }

    [Reactive] public string TxtImageFilePath { get; set; }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        LoadImageCommand = ReactiveCommand.Create(LoadFile);
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        this.WhenAnyValue(x => x.TxtImageFilePath)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Where(str => !string.IsNullOrWhiteSpace(str))
            .Select(str => Cv2.ImRead(str))
            .Do(mat => _imageDataManager.AddImage("Src", mat))
            .Do(mat => _imageDataManager.OutputMatSubject.OnNext(mat))
            .Subscribe()
            .DisposeWith(d);
    }

    protected override void SetupDeactivate()
    {
        base.SetupDeactivate();
    }

    #region PrivateFunction

    private void LoadFile()
    {
        _loadFileConfirm
            .Handle(Unit.Default)
            .Subscribe(str => TxtImageFilePath = str);
    }

    #endregion PrivateFunction
}