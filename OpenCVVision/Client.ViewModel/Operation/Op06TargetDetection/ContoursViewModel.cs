using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Client.ViewModel.Operation
{
    [OperationInfo(6.9, "轮廓", MaterialDesignThemes.Wpf.PackIconKind.VectorPolyline)]
    public class ContoursViewModel : OperaViewModelBase
    {
        private const int THICK3 = 3;
        private Point[][] _contours;
        [Reactive] public string BoundingShapeSelectValue { get; set; }
        public IList<string> BoundingShapesItems { get; set; }
        public IList<string> ContourApproximationModesStr { get; set; }
        [Reactive] public string ContourApproximationSelectValue { get; set; }
        [ObservableAsProperty] public IEnumerable<int> ContourIdItems { get; }
        [Reactive] public int ContourIdItemSelectValue { get; set; }
        public IList<string> RetrievalModesStr { get; set; }
        [Reactive] public string RetrievalSelectValue { get; set; }

        private static void AddShape(Mat dst, IEnumerable<Point> contour, string boundingShape)
        {
            switch (boundingShape)
            {
                case "BoundingRect":
                    Rect rect = Cv2.BoundingRect(contour);
                    Cv2.Rectangle(dst, rect, Scalar.RandomColor(), THICK3);
                    break;

                case "MinAreaRect":
                    if (contour.Count() > 5)
                    {
                        RotatedRect rotateRec = Cv2.MinAreaRect(contour);

                        Point2f[] pts = rotateRec.Points();
                        for (int i = 0; i < 4; i++)
                        {
                            Cv2.Line(dst, pts[i].ToPoint(), pts[(i + 1) % 4].ToPoint(), Scalar.RandomColor(), THICK3);
                        }
                        Point2f pt1 = rotateRec.Center;
                        double ang = rotateRec.Angle / 360d * Cv2.PI * 2;
                        Point pt2 = new((int)(pt1.X + (100 * Math.Sin(ang))), (int)(pt1.Y - (100 * Math.Cos(ang))));
                        Cv2.ArrowedLine(dst, pt1.ToPoint(), pt2, Scalar.RandomColor(), THICK3);
                    }
                    break;

                case "ConvexHull":
                    Point[] pts3 = Cv2.ConvexHull(contour);
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
                        RotatedRect rotateRec1 = Cv2.FitEllipse(contour);
                        Point2f[] pts2 = rotateRec1.Points();
                        for (int i = 0; i < pts2.Length; i++)
                        {
                            Cv2.Line(dst, pts2[i].ToPoint(), pts2[(i + 1) % pts2.Length].ToPoint(), Scalar.RandomColor(), THICK3);
                        }
                        Point2f pt11 = rotateRec1.Center;
                        double ang1 = rotateRec1.Angle / 360d * Cv2.PI * 2;
                        Point pt21 = new((int)(pt11.X + (100 * Math.Sin(ang1))), (int)(pt11.Y - (100 * Math.Cos(ang1))));
                        Cv2.ArrowedLine(dst, pt11.ToPoint(), pt21, Scalar.RandomColor(), THICK3);
                    }
                    break;

                default:
                    break;
            }
        }

        private Point[][] Updateoutput(string restrieval, string contourApproximation)
        {
            SendTime(() =>
            {
                if (restrieval.Equals("FloodFill", StringComparison.Ordinal))
                {
                    Mat dsttmp = _rt.NewMat();
                    _sigleSrc.ConvertTo(dsttmp, MatType.CV_32SC1);
                    _contours = Cv2.FindContoursAsArray(dsttmp, Enum.Parse<RetrievalModes>(restrieval), Enum.Parse<ContourApproximationModes>(contourApproximation));
                }
                else
                {
                    _contours = Cv2.FindContoursAsArray(_sigleSrc, Enum.Parse<RetrievalModes>(restrieval), Enum.Parse<ContourApproximationModes>(contourApproximation));
                }
            }, true);
            return _contours;
        }

        private void UpdateSelectContour(Point[][] contours, int selectIndx, string boundingShape)
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

        protected override void SetupStart()
        {
            base.SetupStart();
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
                .Select(mat => Updateoutput(RetrievalSelectValue, ContourApproximationSelectValue))
                .WhereNotNull()
                .Select(vs => Enumerable.Range(-1, vs.Count() + 1))
                .ToPropertyEx(this, x => x.ContourIdItems)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.RetrievalSelectValue, x => x.ContourApproximationSelectValue)
                .Where(vt => !string.IsNullOrWhiteSpace(vt.Item1) && !string.IsNullOrWhiteSpace(vt.Item2))
                .Where(vt => CanOperat)
                .Select(vt => Updateoutput(RetrievalSelectValue, ContourApproximationSelectValue))
                .WhereNotNull()
                .Select(vs => Enumerable.Range(-1, vs.Count() + 1))
                .ToPropertyEx(this, x => x.ContourIdItems)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.ContourIdItemSelectValue, x => x.BoundingShapeSelectValue)
                .Where(vt => vt.Item1 >= -1 && ContourIdItems != null && ContourIdItems.Any())
                .Where(vt => vt.Item2 != null)
                .Subscribe(vt => UpdateSelectContour(_contours, vt.Item1, vt.Item2))
                .DisposeWith(d);
        }
    }
}