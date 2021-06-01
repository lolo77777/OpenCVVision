using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using LiveChartsCore.Kernel;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel.Operation
{
    [OperationInfo("连通域")]
    public class ConnectedComponentsViewModel : OperaViewModelBase
    {
        [ObservableAsProperty] public int AreaLimit { get; private set; }
        [Reactive] public int AreaMax { get; set; }
        [Reactive] public int AreaMin { get; set; }
        public IEnumerable<string> Filters { get; set; }
        [ObservableAsProperty] public int HeightLimit { get; set; }
        [Reactive] public int HeightMax { get; set; }
        [Reactive] public int HeightMin { get; set; }
        [Reactive] public int LeftMax { get; set; }
        [Reactive] public int LeftMin { get; set; }
        [Reactive] public double Point2dDistance { get; set; }
        [Reactive] public double Point2dX { get; set; }
        [Reactive] public double Point2dY { get; set; }
        [Reactive] public int TopMax { get; set; }
        [Reactive] public int TopMin { get; set; }
        [ObservableAsProperty] public int WidthLimit { get; set; }
        [Reactive] public int WidthMax { get; set; }
        [Reactive] public int WidthMin { get; set; }

        public ConnectedComponentsViewModel()
        {
            this.WhenActivated(d =>
            {
                var currentMatOb = _imageDataManager.InputMatGuidSubject
                    .WhereNotNull()
                    .Where(g => CanOperat)
                    .ObserveOn(RxApp.MainThreadScheduler);

                currentMatOb
                    .Do(g => UpdateOutput(Filters))
                    .Subscribe()
                    .DisposeWith(d);

                currentMatOb
                    .Select(guid => _imageDataManager.GetCurrentMat())
                    .WhereNotNull()
                    .Select(mat => mat.Width)
                    .ToPropertyEx(this, x => x.WidthLimit)
                    .DisposeWith(d);

                currentMatOb
                    .Select(guid => _imageDataManager.GetCurrentMat())
                    .WhereNotNull()
                    .Select(mat => mat.Height)
                    .ToPropertyEx(this, x => x.HeightLimit)
                    .DisposeWith(d);

                currentMatOb
                    .Select(guid => _imageDataManager.GetCurrentMat())
                    .WhereNotNull()
                    .Select(mat => mat.Rows * mat.Cols)
                    .ToPropertyEx(this, x => x.AreaLimit)
                    .DisposeWith(d);
                var areaOb = this.WhenAnyValue(x => x.AreaMax, x => x.AreaMin)
                     .Where(vt => Filters != null && Filters.Count() > 0 && Filters.Any(t => t.Equals("Area")));
                var heightOb = this.WhenAnyValue(x => x.HeightMax, x => x.HeightMin)
                     .Where(vt => Filters != null && Filters.Count() > 0 && Filters.Any(t => t.Equals("Height")));
                var widthOb = this.WhenAnyValue(x => x.WidthMax, x => x.WidthMin)
                     .Where(vt => Filters != null && Filters.Count() > 0 && Filters.Any(t => t.Equals("Width")));
                var leftOb = this.WhenAnyValue(x => x.LeftMax, x => x.LeftMin)
                    .Where(vt => Filters != null && Filters.Count() > 0 && Filters.Any(t => t.Equals("Left")));
                var topOb = this.WhenAnyValue(x => x.TopMax, x => x.TopMin)
                    .Where(vt => Filters != null && Filters.Count() > 0 && Filters.Any(t => t.Equals("Top")));
                var paraOb = Observable.Merge(new[] { areaOb, heightOb, widthOb, leftOb, topOb });
                paraOb
                    .Where(b => CanOperat)
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SubscribeOn(RxApp.TaskpoolScheduler)
                    .Do(b => UpdateOutput(Filters))
                    .Subscribe()
                    .DisposeWith(d);
            });
        }

        private IEnumerable<ConnectedComponents.Blob> FilterBlob(IEnumerable<ConnectedComponents.Blob> blobs, string filterStr, Mat mat)
        {
            IEnumerable<ConnectedComponents.Blob> reBlobs = new List<ConnectedComponents.Blob>();
            switch (filterStr)
            {
                case "Area":
                    reBlobs = blobs.Where(b => b.Area >= AreaMin && b.Area <= AreaMax).ToList();
                    break;

                //case "Centroid":
                //    reBlobs = blobs.Where(b => Point2d.Distance(b.Centroid, new Point2d(Point2dX, Point2dY)) <= Point2dDistance);
                //    break;

                case "Height":
                    reBlobs = blobs.Where(b => b.Height >= HeightMin && b.Height <= HeightMax);
                    break;

                //case "Label":
                //    reBlobs=blobs.Where(b=>b.la)
                //    break;

                case "Left":
                    reBlobs = blobs.Where(b => b.Left >= LeftMin && b.Left <= LeftMax);
                    break;

                //case "Rect":
                //    reBlobs=blobs.Where(b=>b.Rect)
                //    break;

                case "Top":
                    reBlobs = blobs.Where(b => b.Top >= TopMin && b.Top <= TopMax);
                    break;

                case "Width":
                    reBlobs = blobs.Where(b => b.Width >= WidthMin && b.Width <= WidthMax);
                    break;

                default:
                    break;
            }
            return reBlobs;
        }

        private void UpdateOutput(IEnumerable<string> filters = null)
        {
            SendTime(() =>
            {
                var connCom = _sigleSrc.ConnectedComponentsEx();

                IEnumerable<ConnectedComponents.Blob> tmpBlobs1 = connCom.Blobs.ToList();
                IEnumerable<ConnectedComponents.Blob> tmpBlobs2;
                if (filters != null)
                {
                    foreach (string filter in filters)
                    {
                        tmpBlobs2 = new List<ConnectedComponents.Blob>(FilterBlob(tmpBlobs1, filter, _sigleSrc));
                        tmpBlobs1 = new List<ConnectedComponents.Blob>(tmpBlobs2.ToList());
                    }
                }
                var dst = _rt.NewMat();
                if (tmpBlobs1.Any())
                {
                    connCom.FilterByBlobs(_sigleSrc, dst, tmpBlobs1);
                    var dstColor = _rt.T(dst.CvtColor(ColorConversionCodes.GRAY2BGR));
                    tmpBlobs1.ToList().ForEach(blob => dstColor.Rectangle(blob.Rect, Scalar.RandomColor()));

                    _imageDataManager.OutputMatSubject.OnNext(dstColor.Clone());
                }
                else
                {
                    connCom.RenderBlobs(dst);
                    _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
                }
            });
        }
    }
}