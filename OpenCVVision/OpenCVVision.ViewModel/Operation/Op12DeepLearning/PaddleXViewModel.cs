namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(12.2, "PaddleX", "AlphaXCircleOutline")]
public class PaddleXViewModel : OperaViewModelBase
{
    private PaddleXInference _pInfer;
    private ResourcesTracker _resourcesTracker = new();

    private List<string> _clsMatStr = new(16);
    private List<string> _detMatStr = new(8);
    [Reactive] public string ClsResult { get; set; }
    [Reactive] public bool ClsLoaded { get; set; }
    [Reactive] public bool DetLoaded { get; set; }
    public ReactiveCommand<Unit, Unit> LoadClsModelCommand { get; set; }
    public ReactiveCommand<Unit, Unit> DestoryClsModelCommand { get; set; }
    public ReactiveCommand<Unit, Unit> LoadDetModelCommand { get; set; }
    public ReactiveCommand<Unit, Unit> DestoryDetModelCommand { get; set; }

    public PaddleXViewModel()
    {
        //为当前进程添加paddle推理使用到的dll文件
        var paddleDllPath = new[] { Path.Combine(Environment.CurrentDirectory, "PaddleDll") };
        var path = new[] { Environment.GetEnvironmentVariable("PATH") ?? string.Empty };
        string newPath = string.Join(Path.PathSeparator.ToString(), path.Concat(paddleDllPath));
        //这种方式只会修改当前进程的环境变量
        Environment.SetEnvironmentVariable("PATH", newPath);
        _pInfer = new PaddleXInference();
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        _imageDataManager.InputMatGuidSubject
            .Where(guid => ClsLoaded && _imageDataManager.GetImage(guid).TxtMarker.Contains("PaddleX"))
            .Subscribe(_ => UpdateOut());
        _imageDataManager.InputMatGuidSubject
            .Where(guid => DetLoaded && _imageDataManager.GetImage(guid).TxtMarker.Contains("PaddleX"))
            .Subscribe(_ => UpdateOutDet());
    }

    private void UpdateOutDet()
    {
        SendTime(() =>
        {
            var ret = _pInfer.InferImgDet(_src);
            if (ret.IsSuccess)
            {
                var dst = _rt.T(_src.Clone());
                foreach (var reVal in ret.Value.targetRect)
                {
                    var color = Scalar.RandomColor();
                    dst.Rectangle(reVal.Rect, color, 5);
                    dst.PutText(reVal.type, reVal.Rect.Location, HersheyFonts.HersheyPlain, 3, color, 3);
                }
                dst.PutText($"{ret.Value.ms}ms", new Point(50, 50), HersheyFonts.HersheyPlain, 4, Scalar.Red, 3);
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            }
        });
    }

    private void UpdateOut()
    {
        SendTime(async () =>
        {
            var ret = _pInfer.InferImgCls(_src);
            if (ret.IsSuccess)
            {
                _imageDataManager.OutputMatSubject.OnNext(ret.Value.reMat.Clone());
                await Observable.Start(() => ClsResult = ret.Value.reStr, RxApp.MainThreadScheduler);
            }
        });
    }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        LoadClsModelCommand = ReactiveCommand.Create(ClsLoadModel);
        DestoryClsModelCommand = ReactiveCommand.Create(ClsDestoryModel);
        LoadDetModelCommand = ReactiveCommand.Create(DetLoadModel);
        DestoryDetModelCommand = ReactiveCommand.Create(DetDestoryModel);
    }

    private void DetDestoryModel()
    {
        var ret = _pInfer.DestroyModel();
        if (ret.IsSuccess)
        {
            foreach (var mark in _detMatStr)
            {
                if (_imageDataManager.IsExsitByMark(mark))
                {
                    _imageDataManager.RemoveIamge(mark);
                }
            }
            _detMatStr.Clear();
            DetLoaded = false;
        }
    }

    private void ClsDestoryModel()
    {
        var ret = _pInfer.DestroyModel();
        if (ret.IsSuccess)
        {
            foreach (var mark in _clsMatStr)
            {
                if (_imageDataManager.IsExsitByMark(mark))
                {
                    _imageDataManager.RemoveIamge(mark);
                }
            }
            _clsMatStr.Clear();
            ClsLoaded = false;
        }
    }

    private void ClsLoadModel()
    {
        var ret = _pInfer.LoadModel(Path.Combine(Environment.CurrentDirectory, FilePath.Folder.PaddleXClsModelFodel));
        if (ret.IsSuccess)
        {
            ClsLoaded = true;
            DetLoaded = false;
            _clsMatStr.Clear();
            _resourcesTracker = new();
            var srcs = Directory.GetFiles(FilePath.Folder.PaddleXClsImagelFodel).Select(str => _resourcesTracker.T(Cv2.ImRead(str))).ToList();
            for (int i = 0; i < srcs.Count; i++)
            {
                string txtMark = $"PaddleXCls{i}";

                if (!_imageDataManager.IsExsitByMark(txtMark))
                {
                    _imageDataManager.AddImage(txtMark, srcs[i].Clone());
                    _clsMatStr.Add(txtMark);
                }
            }
            srcs.Clear();
            _resourcesTracker.Dispose();
        }
        else
        {
            ClsLoaded = false;
        }
    }

    private void DetLoadModel()
    {
        var ret = _pInfer.LoadModel(Path.Combine(Environment.CurrentDirectory, FilePath.Folder.PaddleXDetModelFodel));
        if (ret.IsSuccess)
        {
            ClsLoaded = false;
            DetLoaded = true;
            _detMatStr.Clear();
            _resourcesTracker = new();
            var srcs = Directory.GetFiles(FilePath.Folder.PaddleXDetImageFodel).Select(str => _resourcesTracker.T(Cv2.ImRead(str))).ToList();
            for (int i = 0; i < srcs.Count; i++)
            {
                string txtMark = $"PaddleXDet{i}";

                if (!_imageDataManager.IsExsitByMark(txtMark))
                {
                    _imageDataManager.AddImage(txtMark, srcs[i].Clone());
                    _detMatStr.Add(txtMark);
                }
            }
            srcs.Clear();
            _resourcesTracker?.Dispose();
        }
        else
        {
            DetLoaded = false;
        }
    }

    protected override void SetupDeactivate()
    {
        if (ClsLoaded)
        {
            ClsDestoryModel();
        }
        if (DetLoaded)
        {
            DetDestoryModel();
        }
        base.SetupDeactivate();
    }
}