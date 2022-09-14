namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(12.3, "WechatQRCode", "QrcodeScan")]
public class WechatQRCodeViewModel : OperaViewModelBase
{
    private WeChatQRCode _qRCode;
    [Reactive] public string DetectResult { get; set; }
    [Reactive] public bool IsLoaded { get; set; }
    public ReactiveCommand<Unit, Unit> LoadModelCommand { get; set; }
    public ReactiveCommand<Unit, Unit> DestroyModelCommand { get; set; }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        _imageDataManager.InputMatGuidSubject
            .Where(_ => IsLoaded)
            .Select(guid => _imageDataManager.GetImage(guid))
            .WhereNotNull()
            .Subscribe(mat => UpdateOutput());
    }

    private void UpdateOutput()
    {
        SendTime(async () =>
        {
            _qRCode.DetectAndDecode(_src, out var box1, out var strs1);
            var dst = _rt.T(_src.Clone());
            var strRet = $"找到{strs1.Length}个二维码";
            if (box1.Any())
            {
                for (int i = 0; i < box1.Length; i++)
                {
                    box1[i].GetArray<float>(out var fs);
                    var rec = new Rect((int)fs[0], (int)fs[1], (int)(fs[2] - fs[0]), (int)(fs[7] - fs[1]));
                    var color = Scalar.RandomColor();
                    Cv2.Rectangle(dst, rec, color, 5);
                    var textSize = Cv2.GetTextSize((i + 1).ToString(), HersheyFonts.HersheySimplex, 2, 5, out var baseline);
                    Cv2.PutText(dst, (i + 1).ToString(), new Point((int)fs[0] + rec.Width / 2 - textSize.Width / 2, (int)fs[1] + rec.Height / 2 + textSize.Height / 2), HersheyFonts.HersheySimplex, 2, color, 5);
                    Cv2.Circle(dst, new Point((int)fs[0] + rec.Width / 2, (int)fs[1] + rec.Height / 2), textSize.Width, color, 5);
                    strRet += Environment.NewLine;
                    strRet += $"第{i + 1}个内容：{strs1[i]}";
                }

                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            }
            await Observable.Start(() => DetectResult = strRet, RxApp.MainThreadScheduler);
        });
    }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        LoadModelCommand = ReactiveCommand.Create(LoadModel);
        DestroyModelCommand = ReactiveCommand.Create(DestroyModel);
    }

    protected override void SetupDeactivate()
    {
        base.SetupDeactivate();
        if (IsLoaded)
        {
            DestroyModel();
        }
    }

    private void LoadModel()
    {
        _qRCode = WeChatQRCode.Create(FilePath.File.DetectorPrototxtPath, FilePath.File.DetectorCaffeModelPath,
            FilePath.File.SuperResolutionPrototxtPath, FilePath.File.SuperResolutionCaffeModelPath);
        _imageDataManager.AddImage("QRCodeTest", Cv2.ImRead(FilePath.Image.QRCodeTest));
        IsLoaded = true;
    }

    private void DestroyModel()
    {
        _qRCode.Dispose();
        _imageDataManager.RemoveIamge("QRCodeTest");
        IsLoaded = false;
    }
}