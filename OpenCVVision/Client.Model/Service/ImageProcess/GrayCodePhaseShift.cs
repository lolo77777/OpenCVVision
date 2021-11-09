using System.Diagnostics;
using System.Threading.Tasks;

namespace Client.Model.Service.ImageProcess
{
    public static class GrayCodePhaseShift
    {
        /// <summary>
        /// 四步相移计算图像相位
        /// </summary>
        /// <param name="p">条纹节距</param>
        /// <returns>该节距时的相位集合</returns>
        private static async Task<Mat> calPhaseMat(List<Mat> listMat, bool display = false)
        {
            int width = listMat[0].Width;
            int height = listMat[0].Height;

            var mat3 = Task.Run(() =>
            {
                var mat2 = new Mat();
                var stopwa = new Stopwatch();
                stopwa.Start();
                var phase = new double[width, height];
                double min = 10000;
                double max = -10000;

                listMat[0].ConvertTo(listMat[0], MatType.CV_32FC1);
                listMat[1].ConvertTo(listMat[1], MatType.CV_32FC1);
                listMat[2].ConvertTo(listMat[2], MatType.CV_32FC1);
                listMat[3].ConvertTo(listMat[3], MatType.CV_32FC1);

                Cv2.Phase(listMat[0] - listMat[2], listMat[3] - listMat[1], mat2);
                var mat = (mat2 / 2f / (float)(Math.PI)).ToMat();
                mat.Row(0).GetArray<float>(out var vs1);
                stopwa.Stop();
                Console.WriteLine($"Mat解相位耗时{stopwa.ElapsedMilliseconds}ms");

                if (display)
                {
                    var gap = 255 / (max - min);
                    var dst = new Mat(height, width, MatType.CV_8UC1);
                    var indexer = dst.GetGenericIndexer<byte>();
                    for (int y = 0; y < dst.Height; y++)
                    {
                        for (int x = 0; x < dst.Width; x++)
                        {
                            indexer[y, x] = (byte)(phase[x, y] * gap);
                        }
                    }
                    Cv2.NamedWindow("dstPhase", WindowFlags.FreeRatio);
                    Cv2.ImShow("dstPhase", dst);
                    Cv2.WaitKey();
                }

                return mat;
            });
            return await mat3;
        }

        private static async Task<(Mat matH, Mat matV)> UnWrapPhaseMat((Mat graycodeH, Mat graycodeV) mat, (Mat phaseMatH, Mat phaseMatV) phase)
        {
            var reMatH = Task.Run(() =>
            {
                var grayMatH = new Mat();

                mat.graycodeH.ConvertTo(grayMatH, MatType.CV_32FC1);

                return (phase.phaseMatH + grayMatH).ToMat();
            });

            var reMatV = Task.Run(() =>
            {
                var grayMatV = new Mat();
                mat.graycodeV.ConvertTo(grayMatV, MatType.CV_32FC1);

                return (phase.phaseMatV + grayMatV).ToMat();
            });

            return (await reMatH, await reMatV);
        }

        private static async Task<Mat> UnWrapPhaseMat(Mat graycode, Mat phaseMat)
        {
            var reMat = Task.Run(() =>
            {
                var grayMat = new Mat();

                graycode.ConvertTo(grayMat, MatType.CV_32FC1);

                return (phaseMat + 4 * grayMat).ToMat();
            });

            return await reMat;
        }

        public static Mat DeGrayCodeMatCal(List<Mat> listmat, bool display = false)
        {
            int width = listmat[0].Width;
            int height = listmat[0].Height;
            var graySrc = new Mat(height, width, MatType.CV_16UC1);
            //await Task.Run(() =>
            //{
            var binMat = new List<Mat>();
            var grayMat = new List<Mat>();
            var dst = new Mat();

            for (int i = 0; i < listmat.Count; i += 2)
            {
                Cv2.Compare(listmat[i], listmat[i + 1], dst, CmpType.GE);
                //Cv2.ImShow("tmp1", listmat[i]);
                //Cv2.ImShow("tmp2", listmat[i + 1]);
                //Cv2.WaitKey();
                var dst1 = new Mat();
                dst1 = dst.Normalize(0, 1, NormTypes.MinMax);
                dst1.ConvertTo(dst1, MatType.CV_16UC1);
                binMat.Add(dst1);
                if (i == 0)
                {
                    grayMat.Add(dst1);
                }
                else
                {
                    var dst2 = new Mat();
                    Cv2.BitwiseXor(grayMat[i / 2 - 1], binMat[i / 2], dst2);
                    dst2.ConvertTo(dst2, MatType.CV_16UC1);

                    grayMat.Add(dst2);
                }
            }
            for (int m = 0; m < grayMat.Count; m++)
            {
                graySrc += grayMat[m] * (short)Math.Pow(2, grayMat.Count - m - 1);
            }

            if (display)
            {
                var newsrc1 = graySrc.Normalize(0, 255, NormTypes.MinMax);
                var newsrc2 = new Mat();
                newsrc1.ConvertTo(newsrc2, MatType.CV_8UC1);
                Cv2.NamedWindow("dstGrayCode", WindowFlags.FreeRatio);
                Cv2.ImShow("dstGrayCode", newsrc2);
                Cv2.WaitKey();
            }
            //});
            binMat.Clear();
            grayMat.Clear();
            dst.Dispose();

            return graySrc;
        }

        public static async Task<(Mat matH, Mat matV)> UnCode(List<Mat> matsH, List<Mat> matsV, List<Mat> matsPH, List<Mat> matsPV)
        {
            var matPh = await calPhaseMat(matsPH);
            matPh.Row(0).GetArray<float>(out var vs);
            var matPv = await calPhaseMat(matsPV);
            matPv.Col(0).GetArray<float>(out var vs1);
            var graMat = (DeGrayCodeMatCal(matsH, false), DeGrayCodeMatCal(matsV, false));
            graMat.Item1.Row(0).GetArray<short>(out var vs2);
            graMat.Item2.Col(0).GetArray<short>(out var vs3);
            var result = await UnWrapPhaseMat((graMat.Item1, graMat.Item2), (matPh, matPv));
            return result;
        }

        public static async Task<Mat> UnCode(List<Mat> mats, List<Mat> matsP)
        {
            var matPh = calPhaseMat(matsP);

            var graMat = DeGrayCodeMatCal(mats, false);
            return await UnWrapPhaseMat(graMat, await matPh);
        }
    }
}