using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
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
    public class ColorSpaceViewModel : ReactiveObject, IOperationViewModel
    {
        private IImageDataManager _imageDataManager;
        private ResourcesTracker _rt = new ResourcesTracker();
        private Mat _src;
        [ObservableAsProperty] public IEnumerable<int> Channels { get; set; }
        [Reactive] public int ChannelSelectInd { get; set; }
        public ReadOnlyCollection<string> ColorModes { get; private set; }
        [Reactive] public int ColorModeSelectInd { get; set; }
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; }

        public ColorSpaceViewModel(IImageDataManager imageDataManager = null)
        {
            _imageDataManager = imageDataManager ?? Locator.Current.GetService<IImageDataManager>();

            ColorModes = new ReadOnlyCollection<string>(new[] { "Gray", "BGR", "HSV", "HLS" });

            this.WhenAnyValue(x => x.ColorModeSelectInd)
                .Where(i => i >= 0)
                .Select(i => i.Equals(0) ? (new[] { 0 }).AsEnumerable() : (new[] { -1, 0, 1, 2 }).AsEnumerable())
                .ToPropertyEx(this, x => x.Channels);

            this.WhenAnyValue(x => x.ColorModeSelectInd, x => x.ChannelSelectInd)
                .Where(i => i.Item1 >= 0 && i.Item2 >= 0 && Channels != null && Channels.Any())
                .Do(i => UpdateOutput(i.Item1, i.Item2))
                .Subscribe();
            _imageDataManager.InputMatGuid
                .WhereNotNull()
                .Delay(TimeSpan.FromMilliseconds(5))
                .Do(guid => _src = _imageDataManager.GetImage(_imageDataManager.CurrentId).ImageMat.Clone())
                .Do(guid => UpdateOutput(ColorModeSelectInd, ChannelSelectInd))
                .Subscribe();
        }

        private void UpdateOutput(int colorModeInd, int channel)
        {
            _src = _rt.T(_imageDataManager.GetImage(_imageDataManager.CurrentId).ImageMat.Clone());
            var dst = _rt.NewMat();
            dst = colorModeInd switch
            {
                0 => _src.CvtColor(ColorConversionCodes.BGR2GRAY),
                1 => channel.Equals(0) ? _src : _src.Split()[channel - 1],
                2 => channel.Equals(0) ? _src : _src.CvtColor(ColorConversionCodes.BGR2HSV).Split()[channel - 1],
                3 => channel.Equals(0) ? _src : _src.CvtColor(ColorConversionCodes.BGR2HLS).Split()[channel - 1],
                _ => _src.Clone()
            };

            _imageDataManager.OutputMat.OnNext(dst.Clone());
            _rt.Dispose();
        }
    }
}