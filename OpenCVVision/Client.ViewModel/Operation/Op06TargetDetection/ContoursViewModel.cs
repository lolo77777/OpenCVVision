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
    [OperationInfo("轮廓")]
    public class ContoursViewModel : OperaViewModelBase
    {
        private Mat<Point>[] _contours;
        public IEnumerable<string> ContourApproximationModesStr { get; set; }
        [Reactive] public string ContourApproximationSelectValue { get; set; }
        public IEnumerable<string> RetrievalModesStr { get; set; }
        [Reactive] public string RetrievalSelectValue { get; set; }

        public ContoursViewModel()
        {
            RetrievalModesStr = Enum.GetNames(typeof(RetrievalModes));
            ContourApproximationModesStr = Enum.GetNames(typeof(ContourApproximationModes));
            this.WhenActivated(d =>
            {
                _imageDataManager.InputMatGuidSubject
                    .Select(guid => _imageDataManager.GetCurrentMat())
                    .WhereNotNull()
                    .Where(mat => CanOperat)
                    .Where(vt => !string.IsNullOrWhiteSpace(RetrievalSelectValue) && !string.IsNullOrWhiteSpace(ContourApproximationSelectValue))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(mat => Updateoutput(RetrievalSelectValue, ContourApproximationSelectValue))
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.RetrievalSelectValue, x => x.ContourApproximationSelectValue)
                    .Where(vt => !string.IsNullOrWhiteSpace(vt.Item1) && !string.IsNullOrWhiteSpace(vt.Item2))
                    .Where(vt => CanOperat)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(vt => Updateoutput(RetrievalSelectValue, ContourApproximationSelectValue))
                    .DisposeWith(d);

                _imageDataManager.RaiseCurrent();
            });
        }

        private void Updateoutput(string restrieval, string contourApproximation)
        {
            SendTime(() =>
            {
                if (restrieval.Equals("FloodFill"))
                {
                    var dsttmp = _rt.NewMat();
                    _sigleSrc.ConvertTo(dsttmp, MatType.CV_32SC1);
                    _contours = Cv2.FindContoursAsMat(dsttmp, Enum.Parse<RetrievalModes>(restrieval), Enum.Parse<ContourApproximationModes>(contourApproximation));
                }
                else
                {
                    _contours = Cv2.FindContoursAsMat(_sigleSrc, Enum.Parse<RetrievalModes>(restrieval), Enum.Parse<ContourApproximationModes>(contourApproximation));
                }

                var dst = _rt.T(_sigleSrc.Clone().CvtColor(ColorConversionCodes.GRAY2BGR));
                Cv2.DrawContours(dst, _contours, -1, Scalar.RandomColor());

                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        private void UpdateSelectContour()
        {
        }
    }
}