using DynamicData;

using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Client.ViewModel.Operation
{
    [OperationInfo(14.1, "特征点检测", MaterialDesignThemes.Wpf.PackIconKind.FeatureHighlight)]
    public class FeatureDetectionViewModel : OperaViewModelBase
    {
        private ReadOnlyObservableCollection<string> _imageItems;
        public IEnumerable<string> FeatureDetectMethodItems { get; private set; }
        public IEnumerable<string> MatchMethodItems { get; private set; }
        [Reactive] public string FeatureDetectMethodSelectValue { get; private set; }
        public ReadOnlyObservableCollection<string> ImageItems => _imageItems;
        public ReactiveCommand<Unit, Unit> MatchCommand { get; private set; }
        [Reactive] public string FirstImageSelectValue { get; private set; }
        [Reactive] public string SecondImageSelectValue { get; private set; }
        [Reactive] public string MatchMethod { get; private set; }
        [Reactive] public bool IsEnableMinDis { get; set; }
        [Reactive] public bool IsEnableRANSAC { get; set; }
        [Reactive] public bool IsEnableKnnMatch { get; set; }

        protected override void SetupStart()
        {
            base.SetupStart();
            FeatureDetectMethodItems = new[] { "AKAZE", "Sift", "Surf", "Brisk", "Orb" };
            MatchMethodItems = new[] { "BfMatcher", "FlannMatcher" };
        }

        protected override void SetupCommands()
        {
            base.SetupCommands();
            var matchCanExe = this.WhenAnyValue(x => x.FirstImageSelectValue, x => x.SecondImageSelectValue, x => x.FeatureDetectMethodSelectValue, x => x.MatchMethod
                    , (fir, sec, feature, match) => fir != null && sec != null && feature != null && match != null && fir != sec);
            MatchCommand = ReactiveCommand.Create(Match, matchCanExe);
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
            _imageDataManager.InputMatGuidSubject
                .Where(guid => CanOperat && !string.IsNullOrWhiteSpace(FeatureDetectMethodSelectValue))
                .Subscribe(guid => UpdateOutput(FeatureDetectMethodSelectValue))
                .DisposeWith(d);
            this.WhenAnyValue(x => x.FeatureDetectMethodSelectValue)
                .WhereNotNull()
                .Subscribe(str => UpdateOutput(str))
                .DisposeWith(d);
            _imageDataManager.SourceCacheImageData
                .Connect()
                .Transform(t => t.TxtMarker)
                .Where(vs => vs.Count >= 2)
                .Bind(out _imageItems)
                .Subscribe()
                .DisposeWith(d);
        }

        #region PrivateFunction

        private void UpdateOutput(string featureDetectMethod)
        {
            SendTime(() =>
            {
                Mat dst = _rt.NewMat();
                Mat descriptors = _rt.NewMat();
                KeyPoint[] kps = null;
                switch (featureDetectMethod)
                {
                    case "AKAZE":
                        AKAZE aKAZE = AKAZE.Create();
                        aKAZE.DetectAndCompute(_src, null, out kps, descriptors);

                        break;

                    case "Sift":
                        SIFT siftSam = SIFT.Create();
                        siftSam.DetectAndCompute(_src, null, out kps, descriptors);
                        break;

                    case "Surf":
                        SURF surfSam = SURF.Create(500);
                        surfSam.DetectAndCompute(_src, null, out kps, descriptors);

                        break;

                    case "Brisk":
                        BRISK briskSam = BRISK.Create();
                        briskSam.DetectAndCompute(_src, null, out kps, descriptors);
                        break;

                    case "Orb":
                        ORB orgSam = ORB.Create();
                        orgSam.DetectAndCompute(_src, null, out kps, descriptors);

                        break;

                    default:
                        break;
                }
                Cv2.DrawKeypoints(_src, kps, dst);
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        private void Match()
        {
            SendTime(() =>
            {
                Mat dst2 = _rt.T(_imageDataManager.GetImage(SecondImageSelectValue).ImageMat.Clone());
                Mat dst1 = _rt.T(_imageDataManager.GetImage(FirstImageSelectValue).ImageMat.Clone());
                Mat reMat = _rt.NewMat();

                Mat descriptors1 = _rt.NewMat();
                Mat descriptors2 = _rt.NewMat();
                KeyPoint[] kps1 = null;
                KeyPoint[] kps2 = null;
                switch (FeatureDetectMethodSelectValue)
                {
                    case "AKAZE":
                        AKAZE aKAZE = AKAZE.Create();
                        aKAZE.DetectAndCompute(dst1, null, out kps1, descriptors1);
                        aKAZE.DetectAndCompute(dst2, null, out kps2, descriptors2);
                        break;

                    case "Sift":
                        SIFT siftSam = SIFT.Create();
                        siftSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                        siftSam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                        break;

                    case "Surf":
                        SURF surfSam = SURF.Create(500);
                        surfSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                        surfSam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                        break;

                    case "Brisk":
                        BRISK briskSam = BRISK.Create();
                        briskSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                        briskSam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                        break;

                    case "Orb":
                        ORB orgSam = ORB.Create();
                        orgSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                        orgSam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                        break;

                    default:
                        break;
                }
                NormTypes type = FeatureDetectMethodSelectValue.Equals("Sift") || FeatureDetectMethodSelectValue.Equals("Surf") ?
                          NormTypes.L2 : NormTypes.Hamming;
                DMatch[] matches = null;

                switch (MatchMethod)
                {
                    case "BfMatcher":
                        BFMatcher bfmatcher = new BFMatcher(type);

                        if (IsEnableKnnMatch)
                        {
                            DMatch[][] matchesKnn = bfmatcher.KnnMatch(descriptors1, descriptors2, 2);

                            matches = matchesKnn.Where(mt => mt[0].Distance < 0.7 * mt[1].Distance).Select(mt => mt[0]).ToArray();
                        }
                        else
                        {
                            matches = bfmatcher.Match(descriptors1, descriptors2);
                        }

                        break;

                    case "FlannMatcher":
                        FlannBasedMatcher flannBasedMatcher = new FlannBasedMatcher();

                        if (descriptors1.Type() != MatType.CV_32F && descriptors2.Type() != MatType.CV_32F)
                        {
                            descriptors1.ConvertTo(descriptors1, MatType.CV_32F);
                            descriptors2.ConvertTo(descriptors2, MatType.CV_32F);
                        }
                        if (IsEnableKnnMatch)
                        {
                            DMatch[][] matchesKnn2 = flannBasedMatcher.KnnMatch(descriptors1, descriptors2, 2);
                            matches = matchesKnn2.Where(mt => mt[0].Distance < 0.7 * mt[1].Distance).Select(mt => mt[0]).ToArray();
                        }
                        else
                        {
                            matches = flannBasedMatcher.Match(descriptors1, descriptors2);
                        }

                        break;

                    default:
                        break;
                }
                DMatch[] dMatchesFilter = null;
                if (IsEnableMinDis)
                {
                    dMatchesFilter = Match_min(matches).ToArray();
                }
                else
                {
                    dMatchesFilter = matches;
                }

                if (dMatchesFilter.Length >= 4 && IsEnableRANSAC)
                {
                    (List<DMatch>, List<Point2d>, List<Point2d>) goodRansac = Ransac(dMatchesFilter, kps1, kps2);
                    Cv2.DrawMatches(dst1, kps1, dst2, kps2, goodRansac.Item1, reMat);
                    IEnumerable<Point2f> ptf1 = goodRansac.Item2.Select(p => new Point2f((float)p.X, (float)p.Y));
                    IEnumerable<Point2f> ptf2 = goodRansac.Item3.Select(p => new Point2f((float)p.X, (float)p.Y));
                    var wh = Cv2.GetAffineTransform(ptf2, ptf1);
                    Mat rematW = _rt.NewMat();

                    Cv2.WarpAffine(dst2, rematW, wh, dst1.Size());
                    _imageDataManager.AddImage("矫正后图像", rematW.Clone());
                }
                else
                {
                    Cv2.DrawMatches(dst1, kps1, dst2, kps2, dMatchesFilter, reMat);
                    IEnumerable<Point2f> ptf1 = dMatchesFilter.Select(t => kps1[t.QueryIdx].Pt);
                    IEnumerable<Point2f> ptf2 = dMatchesFilter.Select(t => kps2[t.TrainIdx].Pt);
                    Mat wh = Cv2.GetAffineTransform(ptf2, ptf1);
                    Mat rematW = _rt.NewMat();

                    Cv2.WarpAffine(dst2, rematW, wh, dst1.Size());
                    _imageDataManager.AddImage("矫正后图像", rematW.Clone());
                }

                _imageDataManager.OutputMatSubject.OnNext(reMat.Clone());
            });
        }

        /// <summary>
        /// 通过最小距离阈值来过滤部分匹配
        /// </summary>
        /// <param name="matches"></param>
        /// <returns></returns>
        private static List<DMatch> Match_min(DMatch[] matches)
        {
            var reList = new List<DMatch>();
            var minDist = 10000f;
            var maxDist = 0f;
            for (int i = 0; i < matches.Length; i++)
            {
                var dist = matches[i].Distance;
                minDist = dist < minDist ? dist : minDist;
                maxDist = dist > maxDist ? dist : maxDist;
            }
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i].Distance <= Math.Max(2 * minDist, 20f))
                {
                    reList.Add(matches[i]);
                }
            }
            return reList;
        }

        /// <summary>
        /// 随机抽取4个点计算单应性矩阵并重投影，比较坐标，记录正确点数量；多次重复，将正确点数量最多的当做正确匹配
        /// </summary>
        /// <param name="dMatches"></param>
        /// <param name="queryKeyPoints"></param>
        /// <param name="trainKeyPoint"></param>
        /// <returns></returns>
        private static (List<DMatch>, List<Point2d>, List<Point2d>) Ransac(DMatch[] dMatches, KeyPoint[] queryKeyPoints, KeyPoint[] trainKeyPoint)
        {
            List<DMatch> reList = new();
            List<Point2d> src1Pts = new();
            List<Point2d> dst1Pts = new();
            List<Point2d> srcPoints = new();
            List<Point2d> dstPoints = new();

            for (int i = 0; i < dMatches.Length; i++)
            {
                srcPoints.Add(new Point2d(queryKeyPoints[dMatches[i].QueryIdx].Pt.X, queryKeyPoints[dMatches[i].QueryIdx].Pt.Y));
                dstPoints.Add(new Point2d(trainKeyPoint[dMatches[i].TrainIdx].Pt.X, trainKeyPoint[dMatches[i].TrainIdx].Pt.Y));
            }
            Mat inliersMask = new Mat();
            Cv2.FindHomography(srcPoints, dstPoints, HomographyMethods.Ransac, 5, inliersMask);
            inliersMask.GetArray<byte>(out var inliersArray);
            for (int i = 0; i < inliersArray.Length; i++)
            {
                if (inliersArray[i] != 0)
                {
                    reList.Add(dMatches[i]);
                    src1Pts.Add(srcPoints[i]);
                    dst1Pts.Add(dstPoints[i]);
                }
            }
            return (reList, src1Pts, dst1Pts);
        }

        #endregion PrivateFunction
    }
}