using Client.ViewModel.Operation;

using ReactiveUI;

using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows.Documents;

namespace Client.View.Operation
{

    /// <summary>
    /// View3DView.xaml 的交互逻辑
    /// </summary>
    public partial class View3DView : ReactiveUserControl<View3DViewModel>
    {
        public View3DView()
        {
            InitializeComponent();
            //ViewModel = Locator.Current.GetService<View3DViewModel>();
            SetupBinding();

        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.NaviBackCommand, v => v.btnNaviBack).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DisplayCommand, v => v.btnDisplay).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CamDx, v => v.viewPort.Camera).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.EffectsManager, v => v.viewPort.EffectsManager).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CamDx.LookDirection, v => v.light.Direction).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PointGeometry, v => v.geometryModel3D.Geometry).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ModelTransform, v => v.geometryModel3D.Transform).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SampleItems, v => v.cbxTestSample.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SampleSelectIndex, v => v.cbxTestSample.SelectedIndex).DisposeWith(d);
                d.Add(viewPort);
                d.Add(light);
                d.Add(geometryModel3D);


            });
        }

        private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start("explorer.exe", Data.FilePath.Folder.PatternFolder);
        }
    }
}