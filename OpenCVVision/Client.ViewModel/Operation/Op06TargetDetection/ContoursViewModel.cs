using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

using Client.Common;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel.Operation
{
    [OperationInfo("轮廓")]
    public class ContoursViewModel : OperaViewModelBase
    {
        private const int THICK3 = 3;
        private IEnumerable<IEnumerable<Point>> _contours;
        [Reactive] public string BoundingShapeSelectValue { get; set; }
        public IEnumerable<string> BoundingShapesItems { get; set; }
        public IEnumerable<string> ContourApproximationModesStr { get; set; }

        [Reactive] public string ContourApproximationSelectValue { get; set; }
        [ObservableAsProperty] public IEnumerable<int> ContourIdItems { get; }
        [Reactive] public int ContourIdItemSelectValue { get; set; }

        public IEnumerable<string> RetrievalModesStr { get; set; }
        [Reactive] public string RetrievalSelectValue { get; set; }

        public ContoursViewModel()
        {
        }

        private void AddShape(Mat dst, IEnumerable<Point> contour, string boundingShape)
        {
            switch (boundingShape)
            {
                case "BoundingRect":
                    var rect = Cv2.BoundingRect(contour);
                    Cv2.Rectangle(dst, rect, Scalar.RandomColor(), THICK3);
                    break;

                case "MinAreaRect":
                    if (contour.Count() > 5)
                    {
                        var rotateRec = Cv2.MinAreaRect(contour);

                        var pts = rotateRec.Points();
                        for (int i = 0; i < 4; i++)
                        {
                            Cv2.Line(dst, pts[i].ToPoint(), pts[(i + 1) % 4].ToPoint(), Scalar.RandomColor(), THICK3);
                        }
                        var pt1 = rotateRec.Center;
                        var ang = (rotateRec.Angle / 360d) * Cv2.PI * 2;
                        var pt2 = new Point((int)(pt1.X + 100 * Math.Sin(ang)), (int)(pt1.Y - 100 * Math.Cos(ang)));
                        Cv2.ArrowedLine(dst, pt1.ToPoint(), pt2, Scalar.RandomColor(), THICK3);
                    }
                    break;

                case "ConvexHull":
                    var pts3 = Cv2.ConvexHull(contour);
                    for (int i = 0; i < pts3.Length; i++)
                    {
                        Cv2.Line(dst, pts3[i], pts3[(i + 1) % pts3.Length], Scalar.RandomColor(), THICK3);
                    }
                    break;

                case "MinEnclosingCircle":
                    Cv2.MinEnclosingCircle(contour, out var ptcen, out var radius);
                    Cv2.Circle(dst, ptcen.ToPoint(), (int)radius, Scalar.RandomColor(), THICK3);
                    break;

                case "MinEnclosingTriangle":
                    Cv2.MinEnclosingTriangle(contour, out var pts1);
                    for (int i = 0; i < pts1.Length; i++)
                    {
                        Cv2.Line(dst, pts1[i].ToPoint(), pts1[(i + 1) % pts1.Length].ToPoint(), Scalar.RandomColor(), THICK3);
                    }
                    break;

                case "FitEllipse":
                    if (contour.Count() > 5)
                    {
                        var rotateRec1 = Cv2.FitEllipse(contour);
                        var pts2 = rotateRec1.Points();
                        for (int i = 0; i < pts2.Length; i++)
                        {
                            Cv2.Line(dst, pts2[i].ToPoint(), pts2[(i + 1) % pts2.Length].ToPoint(), Scalar.RandomColor(), THICK3);
                        }
                        var pt11 = rotateRec1.Center;
                        var ang1 = (rotateRec1.Angle / 360d) * Cv2.PI * 2;
                        var pt21 = new Point((int)(pt11.X + 100 * Math.Sin(ang1)), (int)(pt11.Y - 100 * Math.Cos(ang1)));
                        Cv2.ArrowedLine(dst, pt11.ToPoint(), pt21, Scalar.RandomColor(), THICK3);
                    }

                    break;

                default:
                    break;
            }
        }

        private IEnumerable<IEnumerable<Point>> Updateoutput(string restrieval, string contourApproximation)
        {
            SendTime(() =>
            {
                if (restrieval.Equals("FloodFill"))
                {
                    Mat dsttmp = _rt.NewMat();
                    _sigleSrc.ConvertTo(dsttmp, MatType.CV_32SC1);
                    _contours = Cv2.FindContoursAsArray(dsttmp, Enum.Parse<RetrievalModes>(restrieval), Enum.Parse<ContourApproximationModes>(contourApproximation));
                }
                else
                {
                    _contours = Cv2.FindContoursAsArray(_sigleSrc, Enum.Parse<RetrievalModes>(restrieval), Enum.Parse<ContourApproximationModes>(contourApproximation));
                }
            });
            return _contours;
        }

        private void UpdateSelectContour(IEnumerable<IEnumerable<Point>> contours, int selectIndx, string boundingShape)
        {
            SendTime(() =>
            {
                Mat dst = _rt.T(_sigleSrc.Clone().CvtColor(ColorConversionCodes.GRAY2BGR));

                if (selectIndx == -1)
                {
                    for (int i = 0; i < contours.Count(); i++)
                    {
                        Cv2.DrawContours(dst, contours, i, Scalar.RandomColor(), THICK3);

                        AddShape(dst, contours.ElementAt(i), boundingShape);
                    }
                }
                else
                {
                    Cv2.DrawContours(dst, contours, selectIndx, Scalar.RandomColor(), THICK3);

                    AddShape(dst, contours.ElementAt(selectIndx), boundingShape);
                }

                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        protected override void SetupStart(CompositeDisposable d)
        {
            base.SetupStart(d);
            RetrievalModesStr = Enum.GetNames(typeof(RetrievalModes));
            ContourApproximationModesStr = Enum.GetNames(typeof(ContourApproximationModes));
            BoundingShapesItems = new[] { "BoundingRect", "MinAreaRect", "ConvexHull", "MinEnclosingCircle", "MinEnclosingTriangle", "FitEllipse" };
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);

            _imageDataManager.InputMatGuidSubject
               .Select(guid => _imageDataManager.GetCurrentMat())
               .WhereNotNull()
               .Where(mat => CanOperat)
               .Where(vt => !string.IsNullOrWhiteSpace(RetrievalSelectValue) && !string.IsNullOrWhiteSpace(ContourApproximationSelectValue))
               .ObserveOn(RxApp.MainThreadScheduler)
               .Select(mat => Updateoutput(RetrievalSelectValue, ContourApproximationSelectValue))
               .WhereNotNull()
               .Select(vs => Enumerable.Range(-1, vs.Count() + 1))
               .ToPropertyEx(this, x => x.ContourIdItems)
               .DisposeWith(d);
            this.WhenAnyValue(x => x.RetrievalSelectValue, x => x.ContourApproximationSelectValue)
                .Where(vt => !string.IsNullOrWhiteSpace(vt.Item1) && !string.IsNullOrWhiteSpace(vt.Item2))
                .Where(vt => CanOperat)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(vt => Updateoutput(RetrievalSelectValue, ContourApproximationSelectValue))
                .WhereNotNull()
                .Select(vs => Enumerable.Range(-1, vs.Count() + 1))
                .ToPropertyEx(this, x => x.ContourIdItems)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.ContourIdItemSelectValue, x => x.BoundingShapeSelectValue)
                .Where(vt => vt.Item1 >= -1 && ContourIdItems != null && ContourIdItems.Any())
                .Where(vt => vt.Item2 != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(vt => UpdateSelectContour(_contours, vt.Item1, vt.Item2))
                .DisposeWith(d);

            _imageDataManager.RaiseCurrent();
        }
    }
}