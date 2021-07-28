using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Client.ViewModel.Operation
{
    [OperationInfo(4, "二值化", MaterialDesignThemes.Wpf.PackIconKind.HomeFloorZero)]
    public class ThreshouldViewModel : OperaViewModelBase
    {
        private readonly ObservableCollection<ObservablePoint> _observablePoints = new ObservableCollection<ObservablePoint>();
        [Reactive] public int ChanelSelectIndex { get; private set; }
        [ObservableAsProperty] public IEnumerable<int> Channels { get; private set; }
        [Reactive] public int Maxval { get; private set; }
        [Reactive] public ObservableCollection<ISeries> Series { get; private set; }
        [Reactive] public int Thresh { get; private set; }
        [Reactive] public int Thresh1 { get; private set; }
        [Reactive] public int Thresh2 { get; private set; }
        [Reactive] public string ThresholdSelectValue { get; private set; }
        [Reactive] public bool IsEnableEqualizeHist { get; set; }
        public ReadOnlyCollection<string> ThreshouldModes { get; private set; }

        private void UpdateBar(int channel)
        {
            SendTime(() =>
            {
                Mat reMat = _rt.NewMat();
                var inRanges = new float[2] { 0, 255 };
                var grayMat = _src.Channels() > 0 ? _rt.T(_src.Split()[channel].Clone()) : _rt.T(_src.Clone());
                if (IsEnableEqualizeHist)
                {
                    Cv2.EqualizeHist(grayMat, grayMat);
                }

                Cv2.CalcHist(new[] { grayMat }, new[] { 0 }, null, reMat, 1, new[] { 256 }, new[] { inRanges });
                var dst1 = _rt.T(reMat.Normalize(0, 255, NormTypes.MinMax));
                var dst2 = _rt.NewMat();
                dst1.ConvertTo(dst2, MatType.CV_8UC1);
                dst2.GetArray<byte>(out var vs);
                _observablePoints.Clear();
                int i = -1;
                vs.ToList().ForEach(b => { i++; _observablePoints.Add(new ObservablePoint(i, b)); });
                _imageDataManager.OutputMatSubject.OnNext(grayMat.Clone());
            });
        }

        private void UpdateOutput(double thresh, double setvalue, ThresholdTypes thresholdmethod, int channel)
        {
            SendTime(() =>
            {
                var tmpmat = _rt.NewMat();
                var grayMat = _src.Channels() > 0 ? _rt.T(_src.Split()[channel].Clone()) : _rt.T(_src.Clone());
                switch (thresholdmethod)
                {
                    case ThresholdTypes.Binary:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.Binary);
                        break;

                    case ThresholdTypes.BinaryInv:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.BinaryInv);
                        break;

                    case ThresholdTypes.Trunc:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.Trunc);
                        break;

                    case ThresholdTypes.Tozero:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.Tozero);
                        break;

                    case ThresholdTypes.TozeroInv:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.TozeroInv);
                        break;

                    case ThresholdTypes.Mask:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.Mask);
                        break;

                    case ThresholdTypes.Otsu:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.Otsu);
                        break;

                    case ThresholdTypes.Triangle:
                        tmpmat = grayMat.Threshold(thresh, setvalue, ThresholdTypes.Triangle);
                        break;

                    default:
                        break;
                }
                _imageDataManager.OutputMatSubject.OnNext(tmpmat.Clone());
            });
        }

        private void UpdateOutputFilter(double thresh1, double thresh2, int channel)
        {
            SendTime(() =>
            {
                Mat tmpmat = _rt.NewMat();
                Mat grayMat = _src.Channels() > 0 ? _rt.T(_src.Split()[channel].Clone()) : _rt.T(_src.Clone());
                tmpmat = grayMat.EmptyClone();
                Mat.UnsafeIndexer<byte> grayMatInd = grayMat.GetUnsafeGenericIndexer<byte>();
                Mat.UnsafeIndexer<byte> tmpmatInd = tmpmat.GetUnsafeGenericIndexer<byte>();
                for (int y = 0; y < grayMat.Rows; y++)
                {
                    for (int x = 0; x < grayMat.Cols; x++)
                    {
                        byte tmpValue = grayMatInd[y, x];
                        tmpmatInd[y, x] = tmpValue > thresh1 && tmpValue < thresh2 ? (byte)255 : (byte)0;
                    }
                }
                _imageDataManager.OutputMatSubject.OnNext(tmpmat);
            });
        }

        protected override void SetupStart()
        {
            base.SetupStart();
            ThreshouldModes = new ReadOnlyCollection<string>(Enum.GetNames(typeof(ThresholdTypes)));
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);

            Series = new ObservableCollection<ISeries> { new ColumnSeries<ObservablePoint> { Values = _observablePoints } };

            _imageDataManager.InputMatGuidSubject
                .Throttle(TimeSpan.FromMilliseconds(100))
                .WhereNotNull()
                .Where(guid => CanOperat && ChanelSelectIndex >= 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(guid => UpdateBar(ChanelSelectIndex))
                .Subscribe()
                .DisposeWith(d);
            _imageDataManager.InputMatGuidSubject
                .WhereNotNull()
                .Where(guid => CanOperat && ChanelSelectIndex >= 0 && ThresholdSelectValue != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(guid => UpdateOutput(Thresh, Maxval, (ThresholdTypes)Enum.Parse(typeof(ThresholdTypes), ThresholdSelectValue), ChanelSelectIndex))
                .Subscribe()
                .DisposeWith(d);
            this.WhenAnyValue(x => x.ThresholdSelectValue, x => x.Thresh, x => x.Maxval, x => x.ChanelSelectIndex)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(str => CanOperat && ThresholdSelectValue != null && ChanelSelectIndex >= 0)
                .Do(str => UpdateOutput(Thresh, Maxval, (ThresholdTypes)Enum.Parse(typeof(ThresholdTypes), ThresholdSelectValue), ChanelSelectIndex))
                .Subscribe()
                .DisposeWith(d);
            this.WhenAnyValue(x => x.ChanelSelectIndex, x => x.IsEnableEqualizeHist)
                .Where(vt => vt.Item1 >= 0 && CanOperat)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(i => UpdateBar(ChanelSelectIndex))
                .Subscribe()
                .DisposeWith(d);
            _imageDataManager.InputMatGuidSubject
                .WhereNotNull()
                .Select(src => _imageDataManager.GetCurrentMat())
                .Select(src => Enumerable.Range(0, src.Channels()))
                .Where(vs => vs.Count() > 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(vs => ChanelSelectIndex = 0)
                .ToPropertyEx(this, x => x.Channels)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.Thresh1, x => x.Thresh2)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Where(vt => vt.Item1 >= 0 && vt.Item2 > vt.Item1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(vt => UpdateOutputFilter(vt.Item1, vt.Item2, ChanelSelectIndex))
                .DisposeWith(d);
        }
    }
}