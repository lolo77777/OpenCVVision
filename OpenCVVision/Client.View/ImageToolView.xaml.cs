using Client.ViewModel;

using ReactiveUI;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.View
{
    /// <summary>
    /// ImageToolView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageToolView : ReactiveUserControl<ImageToolViewModel>
    {
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
                this.OneWayBind(ViewModel, vm => vm.DisplayImg, v => v.imgWb.Source).DisposeWith(d);
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
                        double val = (double)arg.Delta / 2000;   //描述鼠标滑轮滚动
                        System.Windows.Point point = arg.GetPosition(imgWb);
                        if (sTran.ScaleX < 0.3 && sTran.ScaleY < 0.3 && arg.Delta < 0)
                        {
                            return;
                        }
                        sTran.CenterX = point.X;
                        sTran.CenterY = point.Y;

                        sTran.ScaleX += val;
                        sTran.ScaleY += val;
                    }).DisposeWith(d);
                imgWb.Events().PreviewMouseDown
                    .Select(args => args.GetPosition(imgWb))
                    .InvokeCommand(this, x => x.ViewModel.MouseDownCommand)
                    .DisposeWith(d);
                imgWb.Events().PreviewMouseMove
                    .Select(args => args.GetPosition(imgWb))
                    .InvokeCommand(this, x => x.ViewModel.MouseMoveCommand)
                    .DisposeWith(d);
            });
        }
    }
}