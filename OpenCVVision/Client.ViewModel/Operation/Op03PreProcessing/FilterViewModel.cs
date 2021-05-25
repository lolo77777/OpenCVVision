using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel.Operation.Op03PreProcessing
{
    [OperationInfo("滤波")]
    public class FilterViewModel : OperaViewModelBase
    {
        [ObservableAsProperty] public bool BolSigmaIsEnable { get; set; }
        public ReadOnlyCollection<string> FilterModes { get; private set; }
        [Reactive] public int FilterModeSelectIndex { get; set; }

        public FilterViewModel()
        {
            FilterModes = new ReadOnlyCollection<string>(new[] { "Blur", "Gaussian", "Median" });
            this.WhenAnyValue(x => x.FilterModeSelectIndex)
                .Select(i => i.Equals(1))
                .ToProperty(this, x => x.BolSigmaIsEnable);
        }

        private void UpdateUi(int filterModeSelectIndex, int kernelSizeX, int kernelSizeY, double sigmaX, double sigmaY)
        {
            var dst = _rt.NewMat();
            dst = filterModeSelectIndex switch
            {
                0 => _src.Blur(new Size(kernelSizeX, kernelSizeY)),
                1 => _src.GaussianBlur(new Size(kernelSizeX, kernelSizeY), sigmaX, sigmaY),
                2 => _src.MedianBlur(kernelSizeX),
                _ => _src.Clone()
            };
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            _rt.Dispose();
        }
    }
}