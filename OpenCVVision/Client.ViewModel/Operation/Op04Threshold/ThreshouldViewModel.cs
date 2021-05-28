using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel.Operation
{
    [OperationInfo("二值化")]
    public class ThreshouldViewModel : OperaViewModelBase
    {
        private ObservableCollection<ObservablePoint> _observablePoints = new ObservableCollection<ObservablePoint>();
        [Reactive] public ObservableCollection<ISeries> Series { get; set; }

        public ThreshouldViewModel()
        {
            this.WhenActivated(d =>
            {
                Series = new ObservableCollection<ISeries> { new ColumnSeries<ObservablePoint> { Values = _observablePoints } };

                _imageDataManager.InputMatGuidSubject
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .WhereNotNull()
                    .Where(guid => CanOperat)
                    .Do(guid => UpdateBar())
                    .Subscribe()
                    .DisposeWith(d);
            });
        }

        private void UpdateBar()
        {
            SendTime(() =>
            {
                Mat reMat = _rt.NewMat();
                var inRanges = new float[2] { 0, 255 };
                var grayMat = _src.Channels() > 1 ? _src.CvtColor(ColorConversionCodes.BGR2GRAY) : _src;
                Cv2.CalcHist(new[] { grayMat }, new[] { 0 }, null, reMat, 1, new[] { 256 }, new[] { inRanges });
                var dst1 = _rt.T(reMat.Normalize(0, 255, NormTypes.MinMax));
                var dst2 = _rt.NewMat();
                dst1.ConvertTo(dst2, MatType.CV_8UC1);
                dst2.GetArray<byte>(out var vs);
                _observablePoints.Clear();
                int i = -1;
                vs.ToList().ForEach(b => { i++; _observablePoints.Add(new ObservablePoint(i, b)); });
            });
        }
    }
}