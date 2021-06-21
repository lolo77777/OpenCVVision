using Client.ViewModel.Operation;

using ReactiveUI;

using Splat;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client.View.Operation
{
    /// <summary>
    /// View3DView.xaml 的交互逻辑
    /// </summary>
    public partial class View3DView : ReactiveWindow<View3DViewModel>
    {
        public View3DView()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<View3DViewModel>();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.DisplayCommand, v => v.btnDisplay).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CamDx, v => v.viewPort.Camera).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.EffectsManager, v => v.viewPort.EffectsManager).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CamDx.LookDirection, v => v.light.Direction).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PointGeometry, v => v.geometryModel3D.Geometry).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ModelTransform, v => v.geometryModel3D.Transform).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SampleItems, v => v.cbxTestSample.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SampleSelectIndex, v => v.cbxTestSample.SelectedIndex).DisposeWith(d);
            });
        }
    }
}