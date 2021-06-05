using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;
using Client.Model.Service;

using DynamicData;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace Client.ViewModel.Operation.Op02ColorSpace
{
    [OperationInfo("色彩空间")]
    public class ColorSpaceViewModel : OperaViewModelBase
    {
        [ObservableAsProperty] public IEnumerable<int> Channels { get; set; }
        [Reactive] public int ChannelSelectInd { get; set; }
        public ReadOnlyCollection<string> ColorModes { get; private set; }
        [Reactive] public int ColorModeSelectInd { get; set; }

        public ColorSpaceViewModel()
        {
        }

        private void UpdateOutput(int colorModeInd, int channel)
        {
            SendTime(() =>
            {
                if (!_src.Channels().Equals(1))
                {
                    var dst = _rt.NewMat();
                    dst = colorModeInd switch
                    {
                        0 => _src.CvtColor(ColorConversionCodes.BGR2GRAY),
                        1 => channel.Equals(0) ? _src : _src.Split()[channel - 1],
                        2 => channel.Equals(0) ? _src : _src.CvtColor(ColorConversionCodes.BGR2HSV).Split()[channel - 1],
                        3 => channel.Equals(0) ? _src : _src.CvtColor(ColorConversionCodes.BGR2HLS).Split()[channel - 1],
                        _ => _src.Clone()
                    };

                    _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
                    _rt.Dispose();
                }
            });
        }

        protected override void SetupStart(CompositeDisposable d)
        {
            base.SetupStart(d);
            CanOperat = _imageDataManager.CurrentId.HasValue ? _imageDataManager.GetCurrentMat().Channels() > 1 : false;
            ColorModes = new ReadOnlyCollection<string>(new[] { "Gray", "BGR", "HSV", "HLS" });
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);

            this.WhenAnyValue(x => x.ColorModeSelectInd)
                .Where(i => i >= 0)
                .Select(i => i.Equals(0) ? (new[] { 0 }).AsEnumerable() : Enumerable.Range(-1, 4))
                .ToPropertyEx(this, x => x.Channels, deferSubscription: true)
                .DisposeWith(d);

            this.WhenAnyValue(x => x.ColorModeSelectInd, x => x.ChannelSelectInd)
                .Where(i => i.Item1 >= 0 && i.Item2 >= 0 && Channels != null && Channels.Any())
                .Where(guid => CanOperat)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(i => UpdateOutput(i.Item1, i.Item2))
                .Subscribe()
                .DisposeWith(d);

            _imageDataManager.InputMatGuidSubject
                .WhereNotNull()
                .Where(guid => CanOperat)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(guid => UpdateOutput(ColorModeSelectInd, ChannelSelectInd))
                .Subscribe()
                .DisposeWith(d);

            _imageDataManager.InputMatGuidSubject
                .WhereNotNull()
                .Select(guid => _imageDataManager.GetCurrentMat().Channels() > 1)
                .BindTo(this, x => x.CanOperat)
                .DisposeWith(d);
            _imageDataManager.RaiseCurrent();
        }
    }
}