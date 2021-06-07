using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Data;

using OpenCvSharp;

namespace Client.Model.Service.ImageProcess
{
    public class LightPlaneCalibrate
    {
        private Size boardSize = new Size(11, 8);
        private string datapath = FilePath.Yaml.LaserLineCaliYaml;
        private float gap = 10f;

        /// <summary>
        /// 灰度质心提取光条中心
        /// </summary>
        /// <param name="roi">光条所在区域</param>
        /// <param name="rowstart">起始行数</param>
        /// <param name="display">是否使用CV窗口显示</param>
        /// <returns></returns>
        private static (Point2d[] laserPoint2f, Point[] laserPoint, Mat mat) GetLineGray(Mat roi, int rowstart, bool display = false)
        {
            Point2d[] relist2F = new Point2d[roi.Width];
            Point[] relist = new Point[roi.Width];
            Mat.UnsafeIndexer<byte> roiindex = roi.GetUnsafeGenericIndexer<byte>();

            for (int x = 0; x < roi.Cols; x++)
            {
                roi.Col(x).MinMaxLoc(out _, out double maxx, out _, out Point maxloc);
                float u = 0, g = 0;
                float location;
                if (maxx > 200)
                {
                    int ymin = maxloc.Y - 20 > 0 ? maxloc.Y - 20 : 0;
                    int ymax = maxloc.Y + 20 > roi.Height ? roi.Height : maxloc.Y + 20;
                    for (int y = ymin; y < ymax; y++)
                    {
                        if (roiindex[y, x] > maxx - 30)
                        {
                            u += roiindex[y, x];
                            g += roiindex[y, x] * y;
                        }
                    }
                    location = g / u;
                }
                else
                {
                    location = -1;
                }

                relist2F[x] = new Point2d(x, rowstart + location);
                relist[x] = new Point(x, rowstart + Math.Round(location, 0));
            }

            Mat remat = roi.CvtColor(ColorConversionCodes.GRAY2BGR);
            Vec3b color = new Vec3b(0, 0, 250);

            foreach (Point p in relist)
            {
                if (p.Y > rowstart)
                {
                    remat.At<Vec3b>(p.Y - rowstart, p.X) = color;
                }
            }
            if (display)
            {
                Cv2.NamedWindow("src", WindowFlags.FreeRatio);
                Cv2.ImShow("src", remat);
                Cv2.WaitKey();
            }

            return (relist2F, relist, remat);
        }

        private void display(Dictionary<string, Mat> mats)
        {
            int i = 1;
            foreach (var m in mats)
            {
                Cv2.NamedWindow(m.Key, WindowFlags.KeepRatio);
                Cv2.ImShow(m.Key, m.Value);
                Cv2.ResizeWindow(m.Key, m.Value.Size());
                i++;
            }

            Cv2.WaitKey(500);
            Cv2.DestroyAllWindows();
        }

        private bool fitPlane(List<Point3f> input, out Mat coeffient)
        {
            bool statu = true;
            coeffient = new Mat();
            Mat dst = new Mat(3, 3, MatType.CV_32F, new Scalar(0));//初始化系数矩阵A
            Mat outM = new Mat(3, 1, MatType.CV_32F, new Scalar(0));//初始化矩阵b
            for (int i = 0; i < input.Count(); i++)
            {
                //计算3*3的系数矩阵
                dst.At<float>(0, 0) = dst.At<float>(0, 0) + (float)Math.Pow(input[i].X, 2);
                dst.At<float>(0, 1) = dst.At<float>(0, 1) + input[i].X * input[i].Y;
                dst.At<float>(0, 2) = dst.At<float>(0, 2) + input[i].X;
                dst.At<float>(1, 0) = dst.At<float>(1, 0) + input[i].X * input[i].Y;
                dst.At<float>(1, 1) = dst.At<float>(1, 1) + (float)Math.Pow(input[i].Y, 2);
                dst.At<float>(1, 2) = dst.At<float>(1, 2) + input[i].Y;
                dst.At<float>(2, 0) = dst.At<float>(2, 0) + input[i].X;
                dst.At<float>(2, 1) = dst.At<float>(2, 1) + input[i].Y;
                dst.At<float>(2, 2) = input.Count;
                //计算3*1的结果矩阵
                outM.At<float>(0, 0) = outM.At<float>(0, 0) + input[i].X * input[i].Z;
                outM.At<float>(1, 0) = outM.At<float>(1, 0) + input[i].Y * input[i].Z;
                outM.At<float>(2, 0) = outM.At<float>(2, 0) + input[i].Z;
            }
            //判断矩阵是否奇异

            double determ = Cv2.Determinant(dst);
            if (Math.Abs(determ) < 0.001)
            {
                statu = false;
                return statu;
            }
            else
            {
                var inv = dst.Inv();

                coeffient = (inv * outM).ToMat();//计算输出
                coeffient.GetArray<float>(out var coeffienttmp);
                return true;
            }
        }

        /// <summary>
        /// 从像素坐标系到相机坐标系
        /// </summary>
        /// <param name="framPoints"></param>
        /// <param name="Rv"></param>
        /// <param name="Tv"></param>
        /// <param name="cameraInMat"></param>
        /// <returns></returns>
        private IEnumerable<Point3f> frameToCamera(IEnumerable<Point2f> framPoints, Vec3d Rv, Vec3d Tv, Mat cameraInMat)
        {
            var rePts = new List<Point3f>();

            var camInMatInv = cameraInMat.Inv();
            var rvtmp = new double[] { Rv.Item0, Rv.Item1, Rv.Item2 };

            Cv2.Rodrigues(rvtmp, out var RvMatrix, out _);
            var RvMat = (new Mat(3, 3, MatType.CV_64FC1, RvMatrix));

            var TvMatrix = new double[] { Tv.Item0, Tv.Item1, Tv.Item2 };

            var RTDouble = new double[16]
           {
                RvMat.At<double>(0,0),RvMat.At<double>(0,1),RvMat.At<double>(0,2),TvMatrix[0],
                RvMat.At<double>(1,0),RvMat.At<double>(1,1),RvMat.At<double>(1,2),TvMatrix[1],
                RvMat.At<double>(2,0),RvMat.At<double>(2,1),RvMat.At<double>(2,2),TvMatrix[2],
                0,0,0,1
           };
            var RTMat = new Mat(4, 4, MatType.CV_64FC1, RTDouble);
            var RTMatInv = RTMat.Inv().ToMat();
            var ap = RTMatInv.At<double>(2, 0); var bp = RTMatInv.At<double>(2, 1); var cp = RTMatInv.At<double>(2, 2); var dp = RTMatInv.At<double>(2, 3);
            var pointpps = new List<Point3d>();
            foreach (var p in framPoints)
            {
                var pm = new Mat(3, 1, MatType.CV_64FC1, new double[] { p.X, p.Y, 1 });
                (camInMatInv * pm).ToMat().GetArray<double>(out var vstmp);
                var pointpp = new Point3d(vstmp[0], vstmp[1], vstmp[2]);
                pointpps.Add(pointpp);
                var xc = (-dp * pointpp.X) / (ap * pointpp.X + bp * pointpp.Y + cp);
                var yc = (-dp * pointpp.Y) / (ap * pointpp.X + bp * pointpp.Y + cp);
                var zc = (-dp) / (ap * pointpp.X + bp * pointpp.Y + cp);
                var pointc = new Point3f((float)xc, (float)yc, (float)zc);
                rePts.Add(pointc);
            }

            return rePts;
        }

        private (Point2f[] laserPoint2f, Point[] laserPoint, Mat mat) GetLine(Mat roi, int rowstart, bool display = false)
        {
            var relist2f = new Point2f[roi.Width];
            var relist = new Point[roi.Width];
            var roiindex = roi.GetUnsafeGenericIndexer<byte>();
            for (int x = 0; x < roi.Cols; x++)
            {
                roi.Col(x).GetArray<byte>(out var vs);

                roi.Col(x).MinMaxLoc(out _, out var maxx, out _, out var maxloc);
                float u = 0, g = 0, location = 0;
                if (maxx > 30)
                {
                    var ymin = maxloc.Y - 30 > 0 ? maxloc.Y - 30 : 0;
                    var ymax = maxloc.Y + 30 > roi.Height ? roi.Height : maxloc.Y + 30;
                    for (int y = ymin; y < ymax; y++)
                    {
                        var tst1 = roiindex[y, x];
                        if (roiindex[y, x] > maxx - 20)
                        {
                            u += roiindex[y, x];
                            g += roiindex[y, x] * y;
                        }
                    }
                    location = g / u;
                }
                else
                {
                    location = -1;
                }

                relist2f[x] = (new Point2f(x, rowstart + location));
                relist[x] = (new Point(x, rowstart + Math.Round(location, 0)));
            }
            var remat = roi.CvtColor(ColorConversionCodes.GRAY2BGR);
            var color = new Vec3b(0, 0, 250);

            foreach (var p in relist)
            {
                if (p.Y > 0)
                {
                    remat.At<Vec3b>(p.Y - rowstart, p.X) = color;
                }
            }
            if (display)
            {
                Cv2.NamedWindow("src", WindowFlags.FreeRatio);
                Cv2.ImShow("src", remat);
                Cv2.WaitKey();
            }
            return (relist2f, relist, remat);
        }

        private (Mat src, Point2f[] laserpoint, Point2f[] laserPointSubmix) getlineWithRec(Mat src, Rect rec, Mat mat = null, bool dis = false)
        {
            var dic = new Dictionary<string, Mat>();
            var dst = new Mat();

            var mask1 = Mat.Zeros(src.Size(), MatType.CV_8UC1).ToMat();
            mask1[rec].SetTo(255);
            src.CopyTo(dst, mask1);

            var result = GetLineGray(dst, 0, false);
            var pts = (from p in result.laserPoint2f
                       where p.Y > 0
                       select new Point2f((float)p.X, (float)p.Y)).ToArray();

            if (dis)
            {
                dic["dst"] = result.mat;
                mat?.Rectangle(rec, new Scalar(255), 4);
                display(dic);
            }
            return (result.mat, pts, null);
        }

        private IEnumerable<Rect> getOutRecBoard(IEnumerable<Point2f[]> point2Fs, IEnumerable<Mat> mats = null, bool dis = false)
        {
            var reRects = new List<Rect>();
            for (int i = 0; i < point2Fs.Count(); i++)
            {
                var rec = Cv2.BoundingRect(point2Fs.ElementAt(i));
                reRects.Add(rec);

                if (dis)
                {
                    var src = mats.ElementAt(i).Clone();
                    src.Rectangle(rec, new Scalar(255), 2);
                    Cv2.NamedWindow("rec", WindowFlags.FreeRatio);
                    Cv2.ImShow("rec", src);
                    Cv2.WaitKey(500);
                    Cv2.DestroyAllWindows();
                }
            }

            return reRects;
        }

        private IEnumerable<Mat> ReadFormFolder(string folderPath)
        {
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                var filelist = Directory.EnumerateFiles(folderPath);

                return from file in filelist
                       select Cv2.ImRead(file, ImreadModes.Grayscale);
            }
            else
            {
                return null;
            }
        }

        public void Calibrate()
        {
            //标定的使用的图片路径
            var folderboard = FilePath.Folder.LaserLineBoardFolder;

            var folderlaser = FilePath.Folder.LaserLineLightFolder;

            var laserMP = new List<(Mat, Point[], Point2f[])>();
            var mats = ReadFormFolder(folderlaser).ToList();
            var lightplaneimgcount = mats.Count();

            //实例化相机标定类
            var cali = new CalibrateCameraHelp(folderboard, boardSize, gap, 800);
            //进行相机标定参数的计算，内参，外参，矫正映射矩阵
            var a = cali.Calibrate(cali.FileMats);

            var newcornersList = new List<Point2f[]>();

            newcornersList = cali.cornersList.GetRange(cali.cornersList.Count - lightplaneimgcount, lightplaneimgcount);
            var matsTmp1 = cali.FileMats.Skip(cali.cornersList.Count - lightplaneimgcount).Take(lightplaneimgcount).Select(t => t.src);
            //圈出角点在图像中的外接矩形区域，只在此区域提取光条中心点

            var rects = getOutRecBoard(newcornersList, matsTmp1, true).ToList();
            var res = (from i in Enumerable.Range(0, mats.Count())
                       select getlineWithRec(mats[i], rects[i], matsTmp1.ElementAt(i), true)).ToList();

            //将光条中心点转化进相机坐标系
            var RvList = cali.Rvecs.AsSpan().Slice(cali.cornersList.Count - lightplaneimgcount, lightplaneimgcount);
            var TvList = cali.Tvecs.AsSpan().Slice(cali.cornersList.Count - lightplaneimgcount, lightplaneimgcount);
            var lightPlanePoint3ds = new List<Point3f>();

            for (int i = 0; i < lightplaneimgcount; i++)
            {
                var laserpoint = res.ElementAt(i).laserpoint;
                var camPoints = frameToCamera(laserpoint, RvList[i], TvList[i], cali.CameraMatrixMat);
                lightPlanePoint3ds.AddRange(camPoints);
            }
            //根据所有光条中心点拟合光平面ax+by+c=z
            var statu = fitPlane(lightPlanePoint3ds, out var coeffient);

            #region 求旋转矩阵

            coeffient.GetArray<float>(out var vs);
            var vec1 = new double[3] { vs[0], vs[1], -1 };
            var vec2 = new double[3] { 0, 1, 0 };
            var r = CalculationRotationMatrix(vec1, vec2);
            var rvMat = new Mat(3, 3, MatType.CV_64FC1, r);

            #endregion 求旋转矩阵

            #region 读写

            //var Rv = cali.Rvecs[4];
            //var Tv = cali.Tvecs[4];
            //var rvtmp = new double[] { Rv.Item0,Rv.Item1,Rv.Item2 };

            //Cv2.Rodrigues(rvtmp,out var RvMatrix,out _);
            //var TvMatrix = new double[] { Tv.Item0,Tv.Item1,Tv.Item2 };
            //var RvMat = new Mat(3,3,MatType.CV_64FC1,RvMatrix);
            //var TvMat = new Mat(3,1,MatType.CV_64FC1,TvMatrix);

            var fw = new FileStorage(datapath, FileStorage.Modes.Write);
            fw.Write("CameraMatrixMat", cali.CameraMatrixMat);
            fw.Write("DiscoeffsMat", cali.DiscoeffsMat);

            fw.Write("LightPlaneCoeffient", coeffient);
            fw.Write("CameraToLightPlaneMat", rvMat);

            fw.Release();

            #endregion 读写
        }

        #region 求旋转矩阵

        private double[,] CalculationRotationMatrix(double[] vectorBefore, double[] vectorAfter)
        {
            double[] rotationAxis;
            double rotationAngle;
            double[,] rotationMatrix;
            rotationAxis = CrossProduct(vectorBefore, vectorAfter);
            rotationAngle = Math.Acos(DotProduct(vectorBefore, vectorAfter) / Normalize(vectorBefore) / Normalize(vectorAfter));
            rotationMatrix = RotationMatrix(rotationAngle, rotationAxis);
            return rotationMatrix;
        }

        private double[] CrossProduct(double[] a, double[] b)
        {
            double[] c = new double[3];

            c[0] = a[1] * b[2] - a[2] * b[1];
            c[1] = a[2] * b[0] - a[0] * b[2];
            c[2] = a[0] * b[1] - a[1] * b[0];

            return c;
        }

        private double DotProduct(double[] a, double[] b)
        {
            double result;
            result = a[0] * b[0] + a[1] * b[1] + a[2] * b[2];

            return result;
        }

        private double Normalize(double[] v)
        {
            double result;

            result = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);

            return result;
        }

        private double[,] RotationMatrix(double angle, double[] u)
        {
            double norm = Normalize(u);
            double[,] rotatinMatrix = new double[3, 3];

            u[0] = u[0] / norm;
            u[1] = u[1] / norm;
            u[2] = u[2] / norm;

            rotatinMatrix[0, 0] = Math.Cos(angle) + u[0] * u[0] * (1 - Math.Cos(angle));
            rotatinMatrix[0, 1] = u[0] * u[1] * (1 - Math.Cos(angle)) - u[2] * Math.Sin(angle);
            rotatinMatrix[0, 2] = u[1] * Math.Sin(angle) + u[0] * u[2] * (1 - Math.Cos(angle));

            rotatinMatrix[1, 0] = u[2] * Math.Sin(angle) + u[0] * u[1] * (1 - Math.Cos(angle));
            rotatinMatrix[1, 1] = Math.Cos(angle) + u[1] * u[1] * (1 - Math.Cos(angle));
            rotatinMatrix[1, 2] = -u[0] * Math.Sin(angle) + u[1] * u[2] * (1 - Math.Cos(angle));

            rotatinMatrix[2, 0] = -u[1] * Math.Sin(angle) + u[0] * u[2] * (1 - Math.Cos(angle));
            rotatinMatrix[2, 1] = u[0] * Math.Sin(angle) + u[1] * u[2] * (1 - Math.Cos(angle));
            rotatinMatrix[2, 2] = Math.Cos(angle) + u[2] * u[2] * (1 - Math.Cos(angle));

            return rotatinMatrix;
        }

        #endregion 求旋转矩阵
    }
}