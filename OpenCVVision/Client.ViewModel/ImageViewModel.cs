using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using DynamicData;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel
{
    public class HistoryItem
    {
        public int HistoryItemId { get; set; }
        public WriteableBitmap HistoryItemImg { get; set; }
    }

    public class ImageViewModel : ReactiveObject
    {
        private ReadOnlyObservableCollection<HistoryItem> _historyItems;
        private SourceCache<HistoryItem, int> _historyItemsTmp = new(t => t.HistoryItemId);
        public ReadOnlyObservableCollection<HistoryItem> HistoryItems => _historyItems;
        [Reactive] public int HistoryItemSelectInd { get; set; }
        public WriteableBitmap InputImg { [ObservableAsProperty] get; }
        public WriteableBitmap OutputImg { [ObservableAsProperty]get; }

        public ImageViewModel()
        {
            _historyItemsTmp.Connect()
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Bind(out _historyItems)
                            .Subscribe();
            this.WhenAnyValue(x => x.HistoryItemSelectInd)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(i => i >= 0 && HistoryItems.Count > i)
                .Select(i => HistoryItems.ElementAt(i).HistoryItemImg)
                .ToPropertyEx(this, x => x.InputImg);
            this.WhenAnyValue(x => x.HistoryItemSelectInd)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(i => i >= 0 && HistoryItems.Count > i)
                .Select(i => HistoryItems.ElementAt(i).HistoryItemImg)
                .ToPropertyEx(this, x => x.OutputImg);
            InitData();
        }

        private void InitData()
        {
            Mat mat = Cv2.ImRead(@"E:\Pictures\高清壁纸Z\ta(1)(1).bmp");
            var scaleY = 90d / mat.Height;
            var dst = mat.Resize(Size.Zero, scaleY, scaleY);
            _historyItemsTmp.AddOrUpdate(new HistoryItem { HistoryItemId = 0, HistoryItemImg = dst.ToWriteableBitmap() });
            var mat1 = Cv2.ImRead(@"E:\Pictures\高清壁纸Z\kaer.jpg");
            var scaleY1 = 90d / mat1.Height;
            var dst1 = mat1.Resize(Size.Zero, scaleY1, scaleY1);
            _historyItemsTmp.AddOrUpdate(new HistoryItem { HistoryItemId = 1, HistoryItemImg = dst1.ToWriteableBitmap() });
        }
    }
}