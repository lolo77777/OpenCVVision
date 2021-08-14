using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

namespace Client.ViewModel.Operation
{
    public class ViewPhotometricViewModel : ViewModelBase, IRoutableViewModel
    {
        private ResourcesTracker _resourcesTracker = new();
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        public string UrlPathSegment { get; }
        public IScreen HostScreen { get; }
        [Reactive] public WriteableBitmap NormalmapWb { get; set; }
        [Reactive] public WriteableBitmap ZWb { get; set; }
        [Reactive] public bool IsRun { get; set; }
        public ReactiveCommand<Unit, Unit> DisplayCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NaviBackCommand { get; set; }
        [Reactive] public int SampleSelectIndex { get; set; }
        public IList<string> SampleItems { get; } = new[] { "buddha", "cat", "gray", "horse", "owl", "rock" };

        public ViewPhotometricViewModel()
        {
            HostScreen = _resolver.GetService<IScreen>("MainHost");
        }

        protected override void SetupCommands()
        {
            base.SetupCommands();
            IObservable<bool> displayCanExecute = this.WhenAnyValue(x => x.SampleSelectIndex, x => x.IsRun, (i, bol) => i >= 0 && !bol);
            DisplayCommand = ReactiveCommand.Create(Display, displayCanExecute);
            IScreen mainScreen = _resolver.GetService<IScreen>("MainHost");
            NaviBackCommand =
                ReactiveCommand.CreateFromObservable(() =>
                mainScreen.Router.Navigate.Execute(_resolver.GetService<ShellViewModel>())
                .Select(_ => Unit.Default));
        }
        protected override void SetupDeactivate()
        {
            base.SetupDeactivate();
            _resourcesTracker.Dispose();
            NormalmapWb = null;
            ZWb = null;
        }
        #region PrivateFunction

        private void Display()
        {
            IsRun = true;
            Observable.Start(() => CalResult(SampleItems.ElementAt(SampleSelectIndex)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(vt =>
                {
                    NormalmapWb = vt.NormalmapMat.ToWriteableBitmap();
                    ZWb = vt.ZMat.ToWriteableBitmap();
                    IsRun = false;
                });
        }

        private (Mat NormalmapMat, Mat ZMat) CalResult(string targetName)
        {
            int nums = 12;
            string CALIBRATION = @$"{Data.FilePath.Folder.PhotometricStereoFolder}\chrome\chrome.";
            string MODEL = @$"{Data.FilePath.Folder.PhotometricStereoFolder}\{targetName}\{targetName}.";

            List<Mat> calibImages = new();
            List<Mat> modelImages = new();
            Mat Lights = _resourcesTracker.T(new Mat(nums, 3, MatType.CV_32F));

            Mat Mask = _resourcesTracker.T(Cv2.ImRead(CALIBRATION + "mask.png", ImreadModes.Grayscale));
            Mat ModelMask = _resourcesTracker.T(Cv2.ImRead(MODEL + "mask.png", ImreadModes.Grayscale));

            Rect bb = GetBoundingBox(Mask);
            var LightsInd = Lights.GetUnsafeGenericIndexer<float>();
            for (int i = 0; i < nums; i++)
            {
                Mat Calib = _resourcesTracker.T(Cv2.ImRead(CALIBRATION + i + ".png", ImreadModes.Grayscale));
                Mat tmp = _resourcesTracker.T(Cv2.ImRead(MODEL + i + ".png", ImreadModes.Grayscale));
                Mat Model = _resourcesTracker.NewMat();
                tmp.CopyTo(Model, ModelMask);
                Vec3f light = GetLightDirFromSphere(Calib, bb);
                LightsInd[i, 0] = light[0];
                LightsInd[i, 1] = light[1];
                LightsInd[i, 2] = light[2];

                calibImages.Add(Calib);
                modelImages.Add(Model);
            }
            int height = calibImages[0].Rows;
            int width = calibImages[0].Cols;
            /* light directions, surface normals, p,q gradients */
            Mat LightsInv = _resourcesTracker.NewMat();
            Cv2.Invert(Lights, LightsInv, DecompTypes.SVD);

            Mat Normals = _resourcesTracker.T(new Mat(height, width, MatType.CV_32FC3, Scalar.All(0)));
            Mat Pgrads = _resourcesTracker.T(new Mat(height, width, MatType.CV_32FC1, Scalar.All(0)));
            Mat Qgrads = _resourcesTracker.T(new Mat(height, width, MatType.CV_32FC1, Scalar.All(0)));
            var normalsIndexer = Normals.GetUnsafeGenericIndexer<Vec3f>();
            var pgradsIndexer = Pgrads.GetUnsafeGenericIndexer<float>();
            var qgradsIndexer = Qgrads.GetUnsafeGenericIndexer<float>();
            /* estimate surface normals and p,q gradients */
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Mat iMat = _resourcesTracker.T(new Mat(nums, 1, MatType.CV_32FC1));
                    var IInd = iMat.GetUnsafeGenericIndexer<float>();
                    for (int i = 0; i < nums; i++)
                    {
                        IInd[i, 0] = modelImages[i].At<byte>(y, x);
                    }

                    Mat n = _resourcesTracker.T(LightsInv * iMat);
                    var nIndexer = n.GetUnsafeGenericIndexer<float>();
                    float p = (float)Math.Sqrt(n.Dot(n));
                    if (p > 0) { n /= p; }
                    if (nIndexer[2, 0] == 0) { nIndexer[2, 0] = 1.0f; }
                    int legit = 1;
                    /* avoid spikes ad edges */

                    for (int i = 0; i < nums; i++)
                    {
                        var modelImageIndexer = modelImages[i].GetUnsafeGenericIndexer<byte>();
                        if (modelImageIndexer[y, x] >= 0)
                        {
                            legit *= 1;
                        }
                        else
                        {
                            legit *= 0;
                        }
                    }
                    if (legit.Equals(1))
                    {
                        n.GetArray<float>(out var vs);

                        normalsIndexer[y, x] = new Vec3f(vs[0], vs[1], vs[2]);
                        pgradsIndexer[y, x] = nIndexer[0, 0] / nIndexer[2, 0];
                        qgradsIndexer[y, x] = nIndexer[1, 0] / nIndexer[2, 0];
                    }
                    else
                    {
                        Vec3f nullvec = new(0.0f, 0.0f, 1.0f);

                        normalsIndexer[y, x] = nullvec;
                        pgradsIndexer[y, x] = 0;
                        qgradsIndexer[y, x] = 0;
                    }
                }
            }
            Mat Normalmap = _resourcesTracker.NewMat();

            Cv2.CvtColor(Normals, Normalmap, ColorConversionCodes.BGR2RGB);
            Normalmap = Normalmap.Normalize(255, 0, NormTypes.MinMax);
            Normalmap.ConvertTo(Normalmap, MatType.CV_8UC3);

            /* global integration of surface normals */
            Mat Z = _resourcesTracker.T(GlobalHeights(Pgrads, Qgrads));

            Mat ZTmp = _resourcesTracker.T(Z.Normalize(255, 0, NormTypes.MinMax));
            Mat ZResult = _resourcesTracker.NewMat();
            ZTmp.ConvertTo(ZResult, MatType.CV_8UC1);
            Cv2.BitwiseNot(ZResult, ZResult);
            return (Normalmap.Clone(), ZResult.Clone());
        }

        private Mat GlobalHeights(Mat Pgrads, Mat Qgrads)
        {
            Mat P = _resourcesTracker.T(new Mat(Pgrads.Rows, Pgrads.Cols, MatType.CV_32FC2, Scalar.All(0)));
            Mat Q = _resourcesTracker.T(new Mat(Pgrads.Rows, Pgrads.Cols, MatType.CV_32FC2, Scalar.All(0)));
            Mat Z = _resourcesTracker.T(new Mat(Pgrads.Rows, Pgrads.Cols, MatType.CV_32FC2, Scalar.All(0)));

            float lambda = 1.0f;
            float mu = 1.0f;

            Cv2.Dft(Pgrads, P, DftFlags.ComplexOutput);
            Cv2.Dft(Qgrads, Q, DftFlags.ComplexOutput);
            var pIndexer = P.GetUnsafeGenericIndexer<Vec2f>();
            var qIndexer = Q.GetUnsafeGenericIndexer<Vec2f>();
            var zIndexer = Z.GetUnsafeGenericIndexer<Vec2f>();
            for (int y = 0; y < Pgrads.Rows; y++)
            {
                for (int x = 0; x < Pgrads.Cols; x++)
                {
                    if (y != 0 || x != 0)
                    {
                        float u = (float)Math.Sin((float)(y * 2 * Cv2.PI / Pgrads.Rows));
                        float v = (float)Math.Sin((float)(x * 2 * Cv2.PI / Pgrads.Cols));

                        float uv = (float)(Math.Pow(u, 2) + Math.Pow(v, 2));
                        float d = (float)((1.0f + lambda) * uv + mu * Math.Pow(uv, 2));

                        var vec0 = (u * pIndexer[y, x][1] + v * qIndexer[y, x][1]) / d;
                        var vec1 = (-u * pIndexer[y, x][0] - v * qIndexer[y, x][0]) / d;
                        zIndexer[y, x] = new Vec2f(vec0, vec1);
                    }
                }
            }
            /* setting unknown average height to zero */

            zIndexer[0, 0] = new Vec2f(0, 0);
            Cv2.Dft(Z, Z, DftFlags.Inverse | DftFlags.Scale | DftFlags.RealOutput);

            return Z;
        }

        /// <summary>
        /// 从标准球的图像标定不同光源的光照方向
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="boundingbox"></param>
        /// <returns></returns>
        private Vec3f GetLightDirFromSphere(Mat Image, Rect boundingbox)
        {
            int THRESH = 254;
            float radius = boundingbox.Width / 2.0f;

            Mat Binary = _resourcesTracker.NewMat();
            Cv2.Threshold(Image, Binary, THRESH, 255, ThresholdTypes.Binary);
            Mat SubImage = _resourcesTracker.T(new Mat(Binary, boundingbox));

            //通过中心距计算亮斑质心

            Moments m = Cv2.Moments(SubImage, false);

            Point center = new(m.M10 / m.M00, m.M01 / m.M00);

            float x = (center.Y - radius) / radius;
            float y = (center.X - radius) / radius;
            float z = (float)Math.Sqrt(1.0 - Math.Pow(x, 2.0) - Math.Pow(y, 2.0));

            return new Vec3f(x, y, z);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Mask"></param>
        /// <returns></returns>
        private Rect GetBoundingBox(Mat Mask)
        {
            Cv2.FindContours(Mask.Clone(), out var v, out var h, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            return Cv2.BoundingRect(v[0]);
        }

        #endregion PrivateFunction
    }
}