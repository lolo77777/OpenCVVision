using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel.Operation
{
    [OperationInfo(3, "滤波", MaterialDesignThemes.Wpf.PackIconKind.Filter)]
    public class FilterViewModel : OperaViewModelBase
    {
        [ObservableAsProperty] public bool BolSigmaColorAndSpace { get; set; }
        [ObservableAsProperty] public bool BolSigmaIsEnable { get; set; }
        [ObservableAsProperty] public bool BolSizeIsEnable { get; set; }
        [ObservableAsProperty] public bool BolSizeYIsEnable { get; set; }
        public ReadOnlyCollection<string> FilterModes { get; private set; }
        [Reactive] public int FilterModeSelectIndex { get; set; }
        [Reactive] public int KernelDiam { get; set; }
        [Reactive] public double SigmaColor { get; set; }
        [Reactive] public double SigmaSpace { get; set; }
        [Reactive] public double SigmaX { get; set; }
        [Reactive] public double SigmaY { get; set; }
        [Reactive] public int SizeX { get; private set; } = 3;
        [Reactive] public int SizeY { get; private set; } = 3;

        private void UpdateUi(int filterModeSelectIndex, int kernelSizeX, int kernelSizeY, double sigmaX, double sigmaY, int kernelDiam, double sigmaColor, double sigmaSpace)
        {
            SendTime(() =>
            {
                if (filterModeSelectIndex > 2)
                {
                    Mat dst = _rt.NewMat();
                    dst = _src.BilateralFilter(kernelDiam, sigmaColor, sigmaSpace);
                    _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
                }
                else
                {
                    Mat dst = _rt.NewMat();
                    dst = filterModeSelectIndex switch
                    {
                        0 => _src.Blur(new Size(kernelSizeX, kernelSizeY)),
                        1 => _src.GaussianBlur(new Size(kernelSizeX, kernelSizeY), sigmaX, sigmaY),
                        2 => _src.MedianBlur(kernelSizeX),
                        _ => _src.Clone()
                    };
                    _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
                }
            });
        }

        protected override void SetupStart(CompositeDisposable d)
        {
            base.SetupStart(d);
            CanOperat = _imageDataManager.CurrentId.HasValue ? _imageDataManager.GetCurrentMat().Channels() > 1 : false;
            FilterModes = new ReadOnlyCollection<string>(new[] { "Blur", "Gaussian", "Median", "BilateralFilter" });
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);

            this.WhenAnyValue(x => x.FilterModeSelectIndex)
                .Select(i => i.Equals(1))
                .ToPropertyEx(this, x => x.BolSigmaIsEnable, deferSubscription: true)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.FilterModeSelectIndex)
                .Select(i => !i.Equals(2))
                .ToPropertyEx(this, x => x.BolSizeYIsEnable, deferSubscription: true)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.FilterModeSelectIndex)
                .Select(i => i.Equals(3))
                .ToPropertyEx(this, x => x.BolSigmaColorAndSpace, deferSubscription: true)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.FilterModeSelectIndex)
                .Select(i => !i.Equals(3))
                .ToPropertyEx(this, x => x.BolSizeIsEnable, deferSubscription: true)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.FilterModeSelectIndex, x => x.SizeX, x => x.SizeY, x => x.SigmaX, x => x.SigmaY)
                .Where(vt => CanOperat)
                .Where(vt => vt.Item1 < 3)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Where(vt => vt.Item1 >= 0 && vt.Item2 > 0 && vt.Item3 > 0)
                .Do(vt => UpdateUi(FilterModeSelectIndex, SizeX, SizeY, SigmaX, SigmaY, KernelDiam, SigmaColor, SigmaSpace))
                .Subscribe()
                .DisposeWith(d);

            this.WhenAnyValue(x => x.FilterModeSelectIndex, x => x.KernelDiam, x => x.SigmaColor, x => x.SigmaSpace)
                .Where(vt => CanOperat)
                .Where(vt => vt.Item1.Equals(3))
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Where(vt => vt.Item1 >= 0 && vt.Item2 > 0 && vt.Item3 > 0)
                .Do(vt => UpdateUi(FilterModeSelectIndex, SizeX, SizeY, SigmaX, SigmaY, KernelDiam, SigmaColor, SigmaSpace))
                .Subscribe()
                .DisposeWith(d);
            _imageDataManager.InputMatGuidSubject
                .WhereNotNull()
                .Where(guid => CanOperat)
                .Do(guid => UpdateUi(FilterModeSelectIndex, SizeX, SizeY, SigmaX, SigmaY, KernelDiam, SigmaColor, SigmaSpace))
                .Subscribe()
                .DisposeWith(d);
            //_imageDataManager.RaiseCurrent();
        }
    }
}