using OpenCVVision.Model.Entity;

using DynamicData.Binding;

using System.Windows.Media.Imaging;

namespace OpenCVVision.ViewModel
{
    public class HistoryMatItem : AbstractNotifyPropertyChanged
    {
        private Guid _itemId;

        //public Guid ItemId
        //{
        //    get { return _itemId; }
        //    set => SetAndRaise(ref _itemId, value);
        //}

        //private Mat _itemImg;

        //public Mat ItemImg
        //{
        //    get { return _itemImg; }
        //    set => SetAndRaise(ref _itemImg, value);
        //}

        //private string _itemTxtMark;

        //public string ItemTxtMark
        //{
        //    get { return _itemTxtMark; }
        //    set => SetAndRaise(ref _itemTxtMark, value);
        //}

        public Guid ItemId { get; set; }
        public Mat ItemImg { get; set; }
        public string ItemTxtMark { get; set; }
    }

    public class ImageViewModel : ViewModelBase
    {
        public readonly SourceCache<HistoryMatItem, Guid> HistoryItemsTmp = new(t => t.ItemId);

        private readonly IImageDataManager _imageDataManager;
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        private readonly ResourcesTracker _rt = new();

        public ReactiveCommand<Unit, Unit> AddOutputImgToImgManagerCommand { get; set; }

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

        private void UpdateHistoryItems(IChangeSet<HistoryMatItem, Guid> changes)
        {
            List<HistoryMatItem> items = changes.Select(t => t.Current).ToList();
            if (changes.Adds > 0 || changes.Updates > 0)
            {
                HistoryItemsTmp.AddOrUpdate(items);
            }
            else if (changes.Removes > 0)
            {
                HistoryItemsTmp.Remove(items);
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
                .Filter(imgdata => imgdata.ImageMat != null && !imgdata.ImageMat.Empty())
                .Transform(it => new HistoryMatItem { ItemId = it.ImageId, ItemTxtMark = it.TxtMarker, ItemImg = it.ImageMat })
                .Subscribe(it => UpdateHistoryItems(it))
                .DisposeWith(d);

            this.WhenAnyValue(x => x.HistoryItemSelectInd)
                .Where(i => i >= 0 && HistoryItemsTmp.Count > i)
                .Select(i => HistoryItemsTmp.KeyValues.ElementAt(i).Value.ItemId)
                .Subscribe(guid => _imageDataManager.InputMatGuidSubject.OnNext(guid))
                .DisposeWith(d);
            _imageDataManager.InputMatGuidSubject
                .Select(guid => _imageDataManager.GetImage(guid).ImageMat)
                .Where(mat => mat != null && !mat.Empty())
                .Do(x => InputImageVM.DisplayMat?.Dispose())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(mat => InputImageVM.DisplayMat = mat.Clone())
                .DisposeWith(d);

            _imageDataManager.OutputMatSubject
                .Where(mat => mat != null && !mat.Empty())
                .Do(x => OutputImageVM.DisplayMat?.Dispose())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(mat => OutputImageVM.DisplayMat = mat.Clone())
                .DisposeWith(d);

            MessageBus.Current.Listen<string>("Time")
                .Select(d => $"{d}ms")
                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(this, x => x.Time)
                .DisposeWith(d);
        }
    }
}