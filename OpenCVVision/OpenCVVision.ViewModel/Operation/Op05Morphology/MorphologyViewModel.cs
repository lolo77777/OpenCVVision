namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(5.2, "形态学1", "MortarPestle")]
public class MorphologyViewModel : OperaViewModelBase
{
    [Reactive] public string MorphShapeSelectValue { get; private set; }
    public IList<string> MorphShapesItems { get; set; }
    [Reactive] public string MorphTypeSelectValue { get; private set; }
    public IList<string> MorphTypesItems { get; private set; }

    [Reactive] public int SizeX { get; private set; }
    [Reactive] public int SizeY { get; private set; }

    private void UpdataOutput(int sizex, int sizey, MorphShapes morphShapes, MorphTypes morphTypes)
    {
        SendTime(() =>
        {
            Mat element = _rt.T(Cv2.GetStructuringElement(morphShapes, new Size(sizex, sizey)));

            Mat reMat = _rt.NewMat();
            Cv2.MorphologyEx(_sigleSrc, reMat, morphTypes, element);

            _imageDataManager.OutputMatSubject.OnNext(reMat.Clone());
        });
    }

    protected override void SetupStart()
    {
        base.SetupStart();
        MorphTypesItems = Enum.GetNames(typeof(MorphTypes));
        MorphShapesItems = Enum.GetNames(typeof(MorphShapes));
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        this.WhenAnyValue(x => x.MorphTypeSelectValue, x => x.MorphShapeSelectValue, x => x.SizeX, x => x.SizeY)
            .Throttle(TimeSpan.FromMilliseconds(150))
            .Where(vt => CanOperat && MorphShapeSelectValue != null && MorphTypeSelectValue != null)
            .Subscribe(vt => UpdataOutput(vt.Item3, vt.Item4, (MorphShapes)Enum.Parse(typeof(MorphShapes), vt.Item2), (MorphTypes)Enum.Parse(typeof(MorphTypes), vt.Item1)))
            .DisposeWith(d);
        _imageDataManager.InputMatGuidSubject
            .WhereNotNull()
            .Where(guid => MorphShapeSelectValue != null && MorphTypeSelectValue != null)
            .Subscribe(guid => UpdataOutput(SizeX, SizeY, (MorphShapes)Enum.Parse(typeof(MorphShapes), MorphShapeSelectValue), (MorphTypes)Enum.Parse(typeof(MorphTypes), MorphTypeSelectValue)))
            .DisposeWith(d);
    }
}