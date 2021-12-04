using Client.Model.Entity;

using OpenCvSharp.WpfExtensions;

using System.Windows.Media.Imaging;

namespace Client.ViewModel
{
    public class HistoryItem
    {
        public Guid HistoryItemId { get; set; }
        public WriteableBitmap HistoryItemImg { get; set; }
        public string HistoryItemTxtMark { get; set; }
    }

    public class ImageViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<HistoryItem> _historyItems;
        private readonly SourceCache<HistoryItem, Guid> _historyItemsTmp = new(t => t.HistoryItemId);
        private readonly IImageDataManager _imageDataManager;
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        private readonly ResourcesTracker _rt = new();

        public ReactiveCommand<Unit, Unit> AddOutputImgToImgManagerCommand { get; set; }
        public ReadOnlyObservableCollection<HistoryItem> HistoryItems => _historyItems;
        [Reactive] public ImageToolViewModel InputImageVM { get; set; }
        [Reactive] public ImageToolViewModel OutputImageVM { get; set; }
        [Reactive] public int HistoryItemSelectInd { get; set; }
        [Reactive] public string OutputImageMarkTxt { get; set; }
        public ReactiveCommand<Unit, bool> RemoveImgFromImgManagerCommand { get; set; }
        [Reactive] public string Time { get; private set; }

        public ImageViewModel(IImageDataManager imageDataManager = null)
        {
            _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
        }

        #region PrivateFunction

        private HistoryItem ConvertData(ImageData imageData)
        {
            WriteableBitmap wtBitmap = MatResizeWt(imageData.ImageMat);

            return new HistoryItem { HistoryItemId = imageData.ImageId, HistoryItemTxtMark = imageData.TxtMarker, HistoryItemImg = wtBitmap };
        }

        private WriteableBitmap MatResizeWt(Mat mat)
        {
            if (mat!=null && !mat.Empty())
            {
                double scaleY = 60d / mat.Height;
                Mat dst = _rt.T(mat.Resize(Size.Zero, scaleY, scaleY));
                WriteableBitmap writeableBitmap = dst.ToWriteableBitmap();
                _rt.Dispose();
                return writeableBitmap;
            }
            else
            {
                return null;
            }
        }

        private void UpdateHistoryItems(IChangeSet<HistoryItem, Guid> changes)
        {
            List<HistoryItem> items = changes.Select(t => t.Current).ToList();
            if (changes.Adds > 0 || changes.Updates > 0)
            {
                _historyItemsTmp.AddOrUpdate(items);
            }
            else if (changes.Removes > 0)
            {
                _historyItemsTmp.Remove(items);
            }
        }

        #endregion PrivateFunction

        protected override void SetupStart()
        {
            base.SetupStart();
            InputImageVM = _resolver.GetService<ImageToolViewModel>();
            OutputImageVM = _resolver.GetService<ImageToolViewModel>();
        }

        protected override void SetupCommands()
        {
            base.SetupCommands();
            AddOutputImgToImgManagerCommand = ReactiveCommand.Create(() => _imageDataManager.AddOutputImage(OutputImageMarkTxt));
            RemoveImgFromImgManagerCommand = ReactiveCommand.Create(() => _imageDataManager.RemoveCurrentImage());
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            _imageDataManager.SourceCacheImageData
                .Connect()
                .Filter(imgdata=> imgdata.ImageMat!=null&&!imgdata.ImageMat.Empty())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(it => ConvertData(it))
                .Subscribe(it => UpdateHistoryItems(it))
                .DisposeWith(d);
            _historyItemsTmp
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _historyItems)
                .Subscribe()
                .DisposeWith(d);
            this.WhenAnyValue(x => x.HistoryItemSelectInd)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(i => i >= 0 && HistoryItems.Count > i)
                .Select(i => HistoryItems.ElementAt(i).HistoryItemId)
                .Subscribe(guid => _imageDataManager.InputMatGuidSubject.OnNext(guid))
                .DisposeWith(d);
            _imageDataManager.InputMatGuidSubject
                .ObserveOn(RxApp.MainThreadScheduler)
                .WhereNotNull()
                .Select(guid => _imageDataManager.GetImage(guid).ImageMat)
                .Subscribe(mat => InputImageVM.DisplayMat = mat)
                .DisposeWith(d);

            _imageDataManager.OutputMatSubject
                .WhereNotNull()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(mat => OutputImageVM.DisplayMat = mat)
                .DisposeWith(d);

            MessageBus.Current.Listen<string>("Time")
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(d => $"{d}ms")
                .BindTo(this, x => x.Time)
                .DisposeWith(d);
        }
    }
}