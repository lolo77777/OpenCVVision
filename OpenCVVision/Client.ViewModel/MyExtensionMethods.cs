using OpenCvSharp;
using OpenCvSharp.Internal.Util;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.ViewModel
{
    public static class MyExtensionMethods
    {
        public static void RenderBlobs(this ConnectedComponents connec, Mat outputMat, IEnumerable<ConnectedComponents.Blob> blobs, ReadOnlyArray2D<int> labels, int blobsTotalCount)
        {
            Mat img = outputMat;
            List<ConnectedComponents.Blob> Blobs = blobs.ToList();
            ReadOnlyArray2D<int> Labels = labels;

            if (img == null)
            {
                throw new ArgumentNullException("img");
            }

            if (Blobs == null || Blobs.Count == 0)
            {
                throw new OpenCvSharpException("Blobs is empty");
            }

            if (Labels == null)
            {
                throw new OpenCvSharpException("Labels is empty");
            }

            int length = Labels.GetLength(0);
            int length2 = Labels.GetLength(1);
            img.Create(new Size(length2, length), MatType.CV_8UC3);
            Scalar[] array = new Scalar[blobsTotalCount];
            array[0] = Scalar.All(0.0);
            for (int i = 1; i < blobsTotalCount; i++)
            {
                array[i] = Scalar.RandomColor();
            }

            int rangesize = length / Environment.ProcessorCount;
            using Mat<Vec3b> mat = new(img);
            MatIndexer<Vec3b> indexer = mat.GetIndexer();

            Parallel.ForEach(Partitioner.Create(0, length, rangesize), l =>
            {
                for (int j = l.Item1; j < l.Item2; j++)
                {
                    for (int k = 0; k < length2; k++)
                    {
                        int num = Labels[j, k];
                        if (blobs.Any(b => b.Label.Equals(num)))
                        {
                            indexer[j, k] = array[num].ToVec3b();
                        }
                    }
                }
            });
        }
    }
}