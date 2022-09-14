using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;



namespace OpenCVVision.View;

[SingleInstanceView]
public partial class ImageView
{
    private ReadOnlyObservableCollection<HistoryImgItem> _historyItems;
    private readonly ResourcesTracker _rt = new();

    public ImageView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.InputImageVM, v => v.InPutImgVM.ViewModel).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.OutputImageVM, v => v.OutPutImgVM.ViewModel).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.HistoryItemSelectInd, v => v.HistoryImg.SelectedIndex).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.AddOutputImgToImgManagerCommand, v => v.btnAddOutputImage).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.OutputImageMarkTxt, v => v.OutputMarkTxtBox.Text).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.RemoveImgFromImgManagerCommand, v => v.btnRemoveImage).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Time, v => v.lblTime.Text).DisposeWith(d);
            this.ViewModel.HistoryItemsTmp
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(x => new HistoryImgItem { HistoryItemId = x.ItemId, HistoryItemImg = MatResizeWt(x.ItemImg), HistoryItemTxtMark = x.ItemTxtMark })
                .Bind(out _historyItems)
                .Subscribe()
                .DisposeWith(d);
            this.WhenAnyValue(x => x._historyItems)
                .BindTo(this, x => x.HistoryImg.ItemsSource)
                .DisposeWith(d);
        });
    }

    private WriteableBitmap MatResizeWt(Mat mat)
    {
        if (mat != null && !mat.Empty())
        {
            double scaleY = 60d / Math.Max(mat.Height, mat.Width);
            Mat dst = _rt.T(mat.Resize(OpenCvSharp.Size.Zero, scaleY, scaleY));
            WriteableBitmap writeableBitmap = dst.ToWriteableBitmap();
            _rt.Dispose();
            return writeableBitmap;
        }
        else
        {
            return null;
        }
    }
}

public class HistoryImgItem
{
    //private Guid _historyItemId;

    //public Guid HistoryItemId
    //{
    //    get { return _historyItemId; }
    //    set => SetAndRaise(ref _historyItemId, value);
    //}

    //private WriteableBitmap _historyItemImg;

    //public WriteableBitmap HistoryItemImg
    //{
    //    get { return _historyItemImg; }
    //    set => SetAndRaise(ref _historyItemImg, value);
    //}

    //private string _historyItemTxtMark;

    //public string HistoryItemTxtMark
    //{
    //    get { return _historyItemTxtMark; }
    //    set => SetAndRaise(ref _historyItemTxtMark, value);
    //}
    public Guid HistoryItemId { get; set; }

    public WriteableBitmap HistoryItemImg { get; set; }
    public string HistoryItemTxtMark { get; set; }
}