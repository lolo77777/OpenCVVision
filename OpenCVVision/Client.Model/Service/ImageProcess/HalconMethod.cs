using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model.Service.ImageProcess
{
    public static class HalconMethod
    {
        #region Emphasize

        public static Mat EmphasizeEx(this Mat src, int maskWidth, int maskHeight, float factor)
        {
            return Emphasize(src, maskWidth, maskHeight, factor);
        }

        public static Mat Emphasize(Mat src, int maskWidth, int maskHeight, float factor)
        {
            Mat mean = new();
            Cv2.Blur(src, mean, new Size(maskWidth, maskHeight));
            Mat dst = src.EmptyClone();
            if (src.Type() == MatType.CV_8UC1)
            {
                var srcIndex = src.GetUnsafeGenericIndexer<byte>();
                var dstIndex = dst.GetUnsafeGenericIndexer<byte>();
                var meanIndex = mean.GetUnsafeGenericIndexer<byte>();

                Parallel.ForEach(Partitioner.Create(0, src.Height, src.Height / Environment.ProcessorCount), (H) =>
                {
                    for (int y = H.Item1; y < H.Item2; y++)
                    {
                        for (int x = 0; x < src.Width; x++)
                        {
                            dstIndex[y, x] = SaturateCast(Math.Round((srcIndex[y, x] - meanIndex[y, x]) * factor) + srcIndex[y, x]);
                        }
                    }
                });
            }
            else if (src.Type() == MatType.CV_8UC3)
            {
                var srcIndex = src.GetUnsafeGenericIndexer<Vec3b>();
                var dstIndex = dst.GetUnsafeGenericIndexer<Vec3b>();
                var meanIndex = mean.GetUnsafeGenericIndexer<Vec3b>();

                Parallel.ForEach(Partitioner.Create(0, src.Height, src.Height / Environment.ProcessorCount), (H) =>
                {
                    for (int y = H.Item1; y < H.Item2; y++)
                    {
                        for (int x = 0; x < src.Width; x++)
                        {
                            var item0 = SaturateCast(Math.Round((srcIndex[y, x].Item0 - meanIndex[y, x].Item0) * factor) + srcIndex[y, x].Item0);
                            var item1 = SaturateCast(Math.Round((srcIndex[y, x].Item1 - meanIndex[y, x].Item1) * factor) + srcIndex[y, x].Item1);
                            var item2 = SaturateCast(Math.Round((srcIndex[y, x].Item2 - meanIndex[y, x].Item2) * factor) + srcIndex[y, x].Item2);

                            dstIndex[y, x] = new Vec3b(item0, item1, item2);
                        }
                    }
                });
            }
            mean.Dispose();
            return dst;
        }

        private static byte SaturateCast(double value)
        {
            return value switch
            {
                < 0 => 0,
                > 255 => 255,
                _ => (byte)value
            };
        }

        #endregion Emphasize
    }
}