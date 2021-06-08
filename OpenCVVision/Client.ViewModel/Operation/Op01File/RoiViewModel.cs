using System;
using System.Collections.Generic;
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
    [OperationInfo(1.5, "ROI", MaterialDesignThemes.Wpf.PackIconKind.RectangleOutline)]
    public class RoiViewModel : OperaViewModelBase
    {
        [Reactive] public int Height { get; private set; }
        [ObservableAsProperty] public int HeightLimit { get; }
        [Reactive] public int Left { get; private set; }
        [Reactive] public string RoiModeSelectValue { get; private set; }
        [Reactive] public int Top { get; private set; }
        [Reactive] public int Width { get; private set; }
        [ObservableAsProperty] public int WidthLimit { get; }

        private void UpdateOutput(Rect rect, string roiMode)
        {
            SendTime(() =>
            {
                var a = new[] { 1, 2, 3, 4, 5 };

                var dst = _rt.NewMat();
                switch (roiMode)
                {
                    case "Roi":

                        dst = _src[rect].Clone();
                        break;

                    case "Mask1":
                        var mask = _rt.T((Mat.Zeros(_src.Size(), MatType.CV_8UC1)).ToMat());
                        mask[rect].SetTo(255);
                        _src.CopyTo(dst, mask);
                        break;

                    case "Mask2":
                        var mask1 = _rt.T(Mat.Zeros(_src.Size(), MatType.CV_8UC1).ToMat());
                        mask1[rect].SetTo(255);
                        _src.CopyTo(dst);
                        dst.SetTo(0, mask1);
                        break;

                    default:
                        break;
                }

                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);

            var currentMatOb = _imageDataManager.InputMatGuidSubject

                .ObserveOn(RxApp.MainThreadScheduler);
            currentMatOb
                .Select(guid => _imageDataManager.GetCurrentMat())
                .WhereNotNull()
                .Select(mat => mat.Width)
                .Do(w => Width = w)
                .ToPropertyEx(this, x => x.WidthLimit)
                .DisposeWith(d);

            currentMatOb
                .Select(guid => _imageDataManager.GetCurrentMat())
                .WhereNotNull()
                .Select(mat => mat.Height)
                .Do(h => Height = h)
                .ToPropertyEx(this, x => x.HeightLimit)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.Left, x => x.Top, x => x.Width, x => x.Height, x => x.RoiModeSelectValue)
                .Throttle(TimeSpan.FromMilliseconds(160))
                .Where(vt => CanOperat && RoiModeSelectValue != null)
                .Where(vt => Left + Width <= WidthLimit && Top + Height <= HeightLimit)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(vt => UpdateOutput(new Rect(vt.Item1, vt.Item2, vt.Item3, vt.Item4), vt.Item5))
                .DisposeWith(d);
        }
    }
}