using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Data;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Client.ViewModel.Operation
{
    [OperationInfo(3.2, "边缘检测", MaterialDesignThemes.Wpf.PackIconKind.MicrosoftEdge)]
    public class EdgeDetectViewModel : OperaViewModelBase
    {
        [Reactive] public bool IsL2gradient { get; set; }
        [Reactive] public int KernelDiam { get; set; }
        [Reactive] public int Threshould1 { get; set; }
        [Reactive] public int Threshould2 { get; set; }

        private void UpdateOutput(int thre1, int thre2, int diam, bool isL2)
        {
            SendTime(() =>
            {
                Mat dst = _rt.NewMat();
                Cv2.Canny(_src, dst, thre1, thre2, diam, isL2);
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
            this.WhenAnyValue(x => x.Threshould1, x => x.Threshould2, x => x.KernelDiam, x => x.IsL2gradient)
                .Where(vt => CanOperat)
                .Subscribe(vt => UpdateOutput(vt.Item1, vt.Item2, vt.Item3, vt.Item4))
                .DisposeWith(d);
        }
    }
}