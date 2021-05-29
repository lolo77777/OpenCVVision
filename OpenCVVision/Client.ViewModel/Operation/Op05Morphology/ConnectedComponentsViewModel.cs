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

namespace Client.ViewModel.Operation
{
    [OperationInfo("连通域")]
    public class ConnectedComponentsViewModel : OperaViewModelBase
    {
        public int AreaMax { get; set; }
        public int AreaMin { get; set; }
        public ReadOnlyCollection<string> Filters { get; private set; }
        public int HeightMax { get; set; }
        public int HeightMin { get; set; }
        public int LeftMax { get; set; }
        public int LeftMin { get; set; }
        public double Point2dDistance { get; set; }
        public double Point2dX { get; set; }
        public double Point2dY { get; set; }
        public int TopMax { get; set; }
        public int TopMin { get; set; }
        public int WidthMax { get; set; }
        public int WidthMin { get; set; }

        public ConnectedComponentsViewModel()
        {
            this.WhenActivated(d =>
            {
                _imageDataManager.InputMatGuidSubject
                    .WhereNotNull()
                    .Where(g => CanOperat)
                    .Do(g => UpdateOutput())
                    .Subscribe()
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.Filters)
                    .WhereNotNull()
                    .Where(vs => vs.Count() > 0)
                    .Do(vs => UpdateOutput(Filters))
                    .Subscribe();
            });
        }

        private IEnumerable<ConnectedComponents.Blob> FilterBlob(IEnumerable<ConnectedComponents.Blob> blobs, string filterStr)
        {
            IEnumerable<ConnectedComponents.Blob> reBlobs = new List<ConnectedComponents.Blob>();
            switch (filterStr)
            {
                case "Area":
                    reBlobs = blobs.Where(b => b.Area >= AreaMin && b.Area <= AreaMax);
                    break;

                case "Centroid":
                    reBlobs = blobs.Where(b => Point2d.Distance(b.Centroid, new Point2d(Point2dX, Point2dY)) <= Point2dDistance);
                    break;

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
                if (filters != null)
                {
                }
            });
        }
    }
}