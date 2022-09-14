using System.Windows.Forms;

namespace OpenCVVision.View.Operation;

/// <summary>
/// YoloView.xaml 的交互逻辑
/// </summary>
public partial class YoloView
{
    private readonly OpenFileDialog openFileDialog = new();

    public YoloView()
    {
        InitializeComponent();
        openFileDialog.Filter = "Weight files or Cfg files (*.weight,*.cfg)|*.weights;*cfg;";
        SetupBinding();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "https://github.com/AlexeyAB/darknet/wiki/YOLOv4-model-zoo");
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            IObservable<string> param1 = Observable.Return("");
            this.OneWayBind(ViewModel, vm => vm.TxtImageFilePath, v => v.FilePathTextBox.Text).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.LoadImageCommand, v => v.btnLoadImage, Observable.Return("weight")).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.LoadImageCommand, v => v.btnLoadCfg, Observable.Return("cfg")).DisposeWith(d);

            this.BindInteraction(ViewModel, vm => vm.LoadFileConfirm,
            context => Observable.Return(openFileDialog.ShowDialog())
                .Do(result =>
                {
                    if (result.Equals(DialogResult.OK))
                    {
                        context.SetOutput(openFileDialog.FileName);
                    }
                    else
                    {
                        context.SetOutput(string.Empty);
                    }
                }))
            .DisposeWith(d);
        });
    }
}