using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OpenCVVision.View;

/// <summary>
/// ImageToolView.xaml 的交互逻辑
/// </summary>
public partial class ImageToolView
{
    private DateTime _lastMidlePressTime = DateTime.Now;
    private WriteableBitmap _writeableBitmapCache;

    public ImageToolView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            base.AddHandler(Button.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ViewModel.Image_MouseUp), true);
            this.Bind(ViewModel, vm => vm.MatInfo, v => v.txtInputImgInfo.Text).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DisplayMat, v => v.imgWb.Source, UpdateWriteableBitmap).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.PathData, v => v.pathDraw.Data).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.TranslateTranX, v => v.tTran.X).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.TranslateTranY, v => v.tTran.Y).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.ColorValue, v => v.txtColorValue.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.IsDrawing, v => v.cbxDrawing.IsChecked).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.GeoSelectIndex, v => v.listBoxDrawGeo.SelectedIndex).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsControlEnable, v => v.sPanelControl.IsEnabled).DisposeWith(d);

            this.BindCommand(ViewModel, vm => vm.AddGeoCommand, v => v.btnAddGeo).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.ClearGeoCommand, v => v.btnClearGeo).DisposeWith(d);
            this.WhenAnyValue(x => x.imgWb.ActualWidth)
                .BindTo(ViewModel, vm => vm.ActualWidth)
                .DisposeWith(d);

            imgWb.Events().MouseWheel
                .Subscribe(arg =>
                {
                    double val = (double)arg.Delta / 3000;   //描述鼠标滑轮滚动
                    System.Windows.Point point = arg.GetPosition(imgWb);
                    if (sTran.ScaleX < 0.3 && sTran.ScaleY < 0.3 && arg.Delta < 0)
                    {
                        return;
                    }
                    sTran.CenterX = point.X;
                    sTran.CenterY = point.Y;

                    sTran.ScaleX += val;
                    sTran.ScaleY += val;
                    var gw = tTran.Value;
                    var g1 = sTran.Value;
                })
                .DisposeWith(d);
            imgWb.Events().PreviewMouseDown
                .Where(args => args.ChangedButton == MouseButton.Left)
                .Select(args => args.GetPosition(imgWb))
                .InvokeCommand(this, x => x.ViewModel.MouseDownCommand)
                .DisposeWith(d);
            imgWb.Events().PreviewMouseMove
                .Select(args => args.GetPosition(imgWb))
                .InvokeCommand(this, x => x.ViewModel.MouseMoveCommand)
                .DisposeWith(d);
            scrollViewer.Events().MouseDown
                .Where(args => args.ChangedButton == MouseButton.Middle)
                .Select(t => (DateTime.Now - _lastMidlePressTime).TotalMilliseconds)
                .Do(t => _lastMidlePressTime = DateTime.Now)
                .Where(i => i < 300)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(e =>
                {
                    sTran.ScaleX = 1;
                    sTran.ScaleY = 1;
                    tTran.X = 0;
                    tTran.Y = 0;
                })
                .DisposeWith(d);
            scrollViewer.Events().MouseWheel
                .Subscribe(args => args.Handled = true)
                .DisposeWith(d);

            this.WhenAnyValue(x => x.ActualWidth)
                .Select(w => w / 5 * 4)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(h => scrollViewer.Height = h)
                .DisposeWith(d);
        });
    }

    private WriteableBitmap UpdateWriteableBitmap(Mat mat)
    {
        if (mat != null && !mat.Empty())
        {
            if (_writeableBitmapCache == null ||
           (int)_writeableBitmapCache.Width != mat.Width ||
           (int)_writeableBitmapCache.Height != mat.Height ||
           _writeableBitmapCache.Format.BitsPerPixel != mat.Channels() * mat.ElemSize() * 8)
            {
                var wb = mat.ToWriteableBitmap();

                _writeableBitmapCache = new WriteableBitmap(mat.Width, mat.Height, 96, 96, wb.Format, wb.Palette);
            }

            _writeableBitmapCache.WritePixels(new System.Windows.Int32Rect(0, 0, mat.Width, mat.Height), mat.Data, mat.Height * mat.Width * mat.Channels(), mat.Width * mat.Channels());
        }
        return _writeableBitmapCache;
    }
}