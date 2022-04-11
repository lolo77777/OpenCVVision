using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Client.ViewModel.Operation;

[OperationInfo(9.1, "模板匹配", "ShieldTemplar")]
public class MatchTemplateViewModel : OperaViewModelBase
{
    private ReadOnlyObservableCollection<string> _imageItems;
    public ReadOnlyObservableCollection<string> ImageItems => _imageItems;
    public IEnumerable<string> MatchMethodItems { get; set; }
    public string MatchMethodSelectValue { get; set; }
    public ReactiveCommand<Unit, Unit> MatchCommand { get; set; }
    [Reactive] public string SrcImageMarkTxt { get; set; }
    [Reactive] public string TemplateImageMarkTxt { get; set; }
    [Reactive] public bool IsEnableAngle { get; private set; }
    [Reactive] public double MatchThreshold { get; private set; }
    [Reactive] public int AngleStep { get; private set; }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        IObservable<bool> matchCanExecute = this.WhenAnyValue(x => x.SrcImageMarkTxt, x => x.TemplateImageMarkTxt, x => x.IsRun,
            (srcStr, TemStr, isrun) => !string.IsNullOrWhiteSpace(srcStr) && !string.IsNullOrWhiteSpace(TemStr) && srcStr != TemStr && !isrun);
        MatchCommand = ReactiveCommand.Create(Match, matchCanExecute);
    }

    protected override void SetupStart()
    {
        base.SetupStart();
        MatchMethodItems = Enum.GetNames<TemplateMatchModes>().Where(name => name.Contains("Normed"));
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        _imageDataManager.SourceCacheImageData
            .Connect()
            .Transform(t => t.TxtMarker)
            .Where(vs => vs.Count >= 2)
            .Bind(out _imageItems)
            .Subscribe()
            .DisposeWith(d);
    }

    #region PrivateFunction

    private void Match()
    {
        SendTime(() =>
        {
            Mat dst1 = _rt.T(_imageDataManager.GetImage(SrcImageMarkTxt).ImageMat.Clone());
            Mat dst2 = _rt.T(_imageDataManager.GetImage(TemplateImageMarkTxt).ImageMat.Clone());
            Mat src = dst1.Width > dst2.Width ? dst1 : dst2;
            Mat template = dst1.Width > dst2.Width ? dst2 : dst1;
            Mat remat = _rt.NewMat();
            IEnumerable<Mat> resultMats = MatchTemplateWithAngele(src, template, Enum.Parse<TemplateMatchModes>(MatchMethodSelectValue), IsEnableAngle, AngleStep);
            IEnumerable<Point> matchLocation = null;
            double min, max;
            switch (Enum.Parse<TemplateMatchModes>(MatchMethodSelectValue))
            {
                case TemplateMatchModes.SqDiffNormed:
                    //范围是0~1,0表示匹配度最大
                    min = 0;
                    max = 1 - 1 * MatchThreshold;
                    matchLocation = GetLocation(min, max, resultMats);
                    break;

                case TemplateMatchModes.CCorrNormed:
                    //范围是0~1,1表示匹配度最大
                    min = 1 * MatchThreshold;
                    max = 1;
                    matchLocation = GetLocation(min, max, resultMats);
                    break;

                case TemplateMatchModes.CCoeffNormed:
                    //范围是-1~1,1表示匹配度最大
                    min = 2 * MatchThreshold - 1;
                    max = 1;
                    matchLocation = GetLocation(min, max, resultMats);
                    break;

                default:
                    break;
            }
            foreach (var pt in matchLocation)
            {
                Cv2.Rectangle(src, new Rect(pt, template.Size()), Scalar.RandomColor());
            }
            _imageDataManager.OutputMatSubject.OnNext(src.Clone());
        });
    }

    private static IEnumerable<Mat> MatchTemplateWithAngele(Mat src, Mat template, TemplateMatchModes templateMatchModes, bool isWithAngle, int angleStep)
    {
        var rt1 = new ResourcesTracker();
        if (!isWithAngle)
        {
            var remat = new Mat();
            Cv2.MatchTemplate(src, template, remat, templateMatchModes);

            return new[] { remat.Clone() };
        }
        else
        {
            var angles = Enumerable.Range(0, 360 / angleStep).Select(i => i * angleStep);
            Mat[] reMats = new Mat[angles.Count()];
            Point2f rotatedCenter = new Point2f(template.Width / 2, template.Height / 2);
            var part = Partitioner.Create(0, angles.Count());
            Parallel.ForEach(part, angleRange =>
            {
                for (int i = angleRange.Item1; i < angleRange.Item2; i++)
                {
                    var angle = angles.ElementAt(i);
                    var remat = rt1.NewMat();
                    var rotatedMat = rt1.T(Cv2.GetRotationMatrix2D(rotatedCenter, angle, 1));
                    Mat newTemplete = rt1.NewMat();
                    Cv2.WarpAffine(template, newTemplete, rotatedMat, template.Size());
                    Cv2.MatchTemplate(src, template, remat, templateMatchModes);
                    reMats[i] = remat.Clone();
                }
            });
            rt1.Dispose();
            return reMats;
        }
    }

    private static IEnumerable<Point> GetLocation(double min, double max, IEnumerable<Mat> matchResult)
    {
        BlockingCollection<Point> rePts = new BlockingCollection<Point>();
        foreach (var mat in matchResult)
        {
            var matIndex = mat.GetUnsafeGenericIndexer<float>();
            var part = Partitioner.Create(0, mat.Rows, mat.Rows / Environment.ProcessorCount);
            Parallel.ForEach(part, rowRange =>
            {
                for (int y = rowRange.Item1; y < rowRange.Item2; y++)
                {
                    for (int x = 0; x < mat.Cols; x++)
                    {
                        if (matIndex[y, x] >= min && matIndex[y, x] <= max)
                        {
                            rePts.Add(new Point(x, y));
                        }
                    }
                }
            });
        }
        return rePts.ToList();
    }

    #endregion PrivateFunction
}