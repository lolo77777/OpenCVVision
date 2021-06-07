using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

namespace Client.Model.Service.ImageProcess
{
    internal class CalibrateCameraHelp
    {
        private Size _boardSize;
        private int _displayWidth;
        private string _folderPath;
        private float _gap;
        private int _imageCount;
        private Size _srcSize;
        public double[,] CameraMatrix = new double[3, 3];
        public List<Point2f[]> cornersList = new();
        public double[] DistCoeffs = new double[5];
        public List<Point3f[]> objectspointList = new();
        public Mat CameraMatrixMat { set; get; }
        public Mat DiscoeffsMat { set; get; }
        public List<(string name, Mat src)> FileMats { set; get; } = new List<(string name, Mat src)>();
        public List<string> FilesPahtList { set; get; }
        public Mat Map1 { set; get; } = new Mat();
        public Mat Map2 { set; get; } = new Mat();
        public Vec3d[] Rvecs { set; get; }
        public Vec3d[] Tvecs { set; get; }

        /// <summary>
        /// 实例化标定模型，读取本地文件标定
        /// </summary>
        /// <param name="folderPath">图片文件夹</param>
        /// <param name="boardSize">标定板内角点Size</param>
        /// <param name="gap">标定板方格物理边长</param>
        /// <param name="displayWidth">标定过程中显示窗口宽度</param>
        public CalibrateCameraHelp(string folderPath, Size boardSize, float gap, int displayWidth)
        {
            this._folderPath = folderPath;
            this._displayWidth = displayWidth;
            this._boardSize = boardSize;
            Rvecs = new Vec3d[_imageCount];
            Tvecs = new Vec3d[_imageCount];
            this._gap = gap;
            Init();
        }

        /// <summary>
        /// 实例化标定模型,在线标定
        /// </summary>
        /// <param name="folderPath">图片文件夹</param>
        /// <param name="boardSize">标定板内角点Size</param>
        /// <param name="gap">标定板方格物理边长</param>
        /// <param name="displayWidth">标定过程中显示窗口宽度</param>
        public CalibrateCameraHelp(Size boardSize, float gap, int imgcount)
        {
            _imageCount = imgcount;
            this._boardSize = boardSize;
            Rvecs = new Vec3d[_imageCount];
            Tvecs = new Vec3d[_imageCount];
            this._gap = gap;
            Init();
        }

        /// <summary>
        /// 无参构造，只能用于读取已保存的标定信息
        /// </summary>
        public CalibrateCameraHelp()
        {
        }

        private double GetErr(Point2f[] point2Fs1, Point2f[] point2Fs2)

        {
            double d = 0;
            for (int i = 0; i < point2Fs2.Length; i++)
            {
                d += Point2f.Distance(point2Fs1[i], point2Fs2[i]);
            }
            return d / point2Fs2.Length;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            if (!string.IsNullOrWhiteSpace(_folderPath))
            {
                FilesPahtList = Directory.EnumerateFiles(_folderPath).ToList();
                _imageCount = FilesPahtList.Count;
                FilesPahtList.ForEach(p => FileMats.Add((p.Replace(_folderPath, ""), Cv2.ImRead(p))));
            }

            for (int i = 0; i < _imageCount; i++)
            {
                var objectpoints = new Point3f[_boardSize.Width * _boardSize.Height];
                for (int j = 0; j < _boardSize.Height; j++)
                {
                    for (int m = 0; m < _boardSize.Width; m++)
                    {
                        objectpoints[j * _boardSize.Width + m] = new Point3f(m * _gap, j * _gap, 0);
                    }
                }
                objectspointList.Add(objectpoints);
            }
        }

        /// <summary>
        /// 找角点，计算内参
        /// </summary>
        /// <param name="display">是否显示角点图像</param>
        /// <param name="autoCloseTime">图像显示的时间，单位ms</param>
        public List<(string name, Mat src)> Calibrate(IEnumerable<(string name, Mat src)> orginMats)
        {
            var dsts = new List<(string name, Mat src)>();
            for (int i = 0; i < orginMats.Count(); i++)
            {
                var src = orginMats.ElementAt(i).src;

                var gray = src.EmptyClone();
                if (src.Channels() != 1)
                {
                    Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                }
                else
                {
                    gray = src.Clone();
                }

                _srcSize = src.Size();
                var imagepoints = new Point2f[_boardSize.Width * _boardSize.Height];

                if (Cv2.FindChessboardCorners(gray, _boardSize, out imagepoints))
                {
                    Cv2.Find4QuadCornerSubpix(gray, imagepoints, new Size(5, 5));

                    cornersList.Add(imagepoints);
                    var cornerSrc = src.Clone();
                    Cv2.DrawChessboardCorners(cornerSrc, _boardSize, imagepoints, true);
                    dsts.Add((orginMats.ElementAt(i).name, cornerSrc));
                    Cv2.NamedWindow(orginMats.ElementAt(i).name, WindowFlags.FreeRatio);
                    Cv2.ImShow(orginMats.ElementAt(i).name, cornerSrc);
                    Cv2.WaitKey(500);
                    Cv2.DestroyAllWindows();
                }
                else
                {
                    objectspointList.RemoveAt(0);
                }

                gray.Dispose();
            }

            if (cornersList.Count > 0)
            {
                Vec3d[] rvecs = new Vec3d[_imageCount];
                Vec3d[] tvecs = new Vec3d[_imageCount];

                Cv2.CalibrateCamera(objectspointList, cornersList, _srcSize,
                    CameraMatrix, DistCoeffs, out rvecs, out tvecs);
                Rvecs = rvecs;
                Tvecs = tvecs;
                CameraMatrixMat = new Mat(3, 3, MatType.CV_64FC1, CameraMatrix);
                DiscoeffsMat = new Mat(1, 5, MatType.CV_64FC1, DistCoeffs);

                Cv2.InitUndistortRectifyMap(CameraMatrixMat, DiscoeffsMat, new Mat(), new Mat(), _srcSize, MatType.CV_32FC1, Map1, Map2);
            }

            return dsts;
        }

        /// <summary>
        /// 找角点，计算内参
        /// </summary>
        /// <param name="display">是否显示角点图像</param>
        /// <param name="autoCloseTime">图像显示的时间，单位ms</param>
        public void CalibrateOffline(bool display, int autoCloseTime = 10)
        {
            for (int i = 0; i < _imageCount; i++)
            {
                var path1 = FilesPahtList[i];
                var src = Cv2.ImRead(path1);
                var gray = src.EmptyClone();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                _srcSize = src.Size();
                var imagepoints = new Point2f[_boardSize.Width * _boardSize.Height];

                if (Cv2.FindChessboardCorners(gray, _boardSize, out imagepoints))
                {
                    Cv2.Find4QuadCornerSubpix(gray, imagepoints, new Size(11, 11));

                    cornersList.Add(imagepoints);
                    if (display)
                    {
                        Cv2.NamedWindow("src", WindowFlags.FreeRatio);
                        Cv2.NamedWindow("Corner", WindowFlags.FreeRatio);
                        Cv2.ResizeWindow("src", new Size(_displayWidth, _displayWidth));
                        Cv2.ResizeWindow("Corner", new Size(_displayWidth, _displayWidth));
                        Cv2.MoveWindow("src", 50, 50);
                        Cv2.MoveWindow("Corner", _displayWidth + 100, 50);
                        var cornerSrc = src.Clone();
                        Cv2.DrawChessboardCorners(cornerSrc, _boardSize, imagepoints, true);
                        Cv2.ImShow("src", src);
                        Cv2.ImShow("Corner", cornerSrc);

                        var a = Cv2.WaitKeyEx(autoCloseTime);

                        cornerSrc.Dispose();
                    }
                }
                else
                {
                    Console.WriteLine($"{path1}找角点失败！");
                }

                src.Dispose();
                gray.Dispose();
            }
            Cv2.DestroyAllWindows();
            Vec3d[] rvecs = new Vec3d[_imageCount];
            Vec3d[] tvecs = new Vec3d[_imageCount];
            Cv2.CalibrateCamera(objectspointList, cornersList, _srcSize,
                CameraMatrix, DistCoeffs, out rvecs, out tvecs);

            Rvecs = rvecs;
            Tvecs = tvecs;
            CameraMatrixMat = new Mat(3, 3, MatType.CV_64FC1, CameraMatrix);
            DiscoeffsMat = new Mat(1, 5, MatType.CV_64FC1, DistCoeffs);
        }

        /// <summary> 显示畸变矫正之后的图像 </summary> <param name="time">图像显示时间(ms)param>
        public void DisplayRemap(int time)
        {
            for (int i = 0; i < _imageCount; i++)
            {
                var path1 = FilesPahtList[i];
                var src = Cv2.ImRead(path1);
                var dst = src.EmptyClone();
                //var dst2 = src.EmptyClone();
                //Cv2.Undistort(src,dst2,CameraMatrixMat,DiscoeffsMat);

                Cv2.Remap(src, dst, Map1, Map2);
                Cv2.NamedWindow("src", WindowFlags.FreeRatio);
                Cv2.NamedWindow("dst", WindowFlags.FreeRatio);
                //Cv2.NamedWindow("dst2",WindowMode.FreeRatio);
                Cv2.ResizeWindow("src", new Size(_displayWidth, _displayWidth));
                Cv2.ResizeWindow("dst", new Size(_displayWidth, _displayWidth));
                //Cv2.ResizeWindow("dst2",new Size(400,400));

                Cv2.MoveWindow("src", 50, 50);
                Cv2.MoveWindow("dst", _displayWidth + 100, 50);
                //Cv2.MoveWindow("dst2",900,0);
                Cv2.ImShow("src", src);
                Cv2.ImShow("dst", dst);
                //Cv2.ImShow("dst2",dst2);
                Cv2.WaitKeyEx(time);
                src.Dispose();
                dst.Dispose();
            }
            Cv2.DestroyAllWindows();
        }

        /// <summary>
        /// 评价标定结果
        /// </summary>
        public void EvaluateCalibrateResult()
        {
            double errsum = 0.0;
            for (int i = 0; i < _imageCount; i++)
            {
                var imagepoint2 = new Point2f[_boardSize.Width * _boardSize.Height];
                //var jacobian = new double[11 * 8,10];
                Cv2.ProjectPoints(objectspointList[i], Vec3d2doubleArray(Rvecs[i]), Vec3d2doubleArray(Tvecs[i])
                    , CameraMatrix, DistCoeffs, out imagepoint2, out _);
                var src1 = new Mat(_boardSize.Width, _boardSize.Height, MatType.CV_64FC1, imagepoint2);
                var src2 = new Mat(_boardSize.Width, _boardSize.Height, MatType.CV_64FC1, cornersList[i]);
                var err1 = Cv2.Norm(src1, src2);
                var err = GetErr(imagepoint2, cornersList[i]);
                errsum += err;
                Console.WriteLine($"第{i + 1}幅图像误差为{err}个像素");
            }
            Console.WriteLine($"平均误差为{errsum / _imageCount}个像素");
        }

        /// <summary>
        /// 控制台输出标定结果
        /// </summary>
        public string PrintCalibrateResult()
        {
            var strRe = new StringBuilder();
            for (int i = 0; i < CameraMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < CameraMatrix.GetLength(1); j++)
                {
                    strRe.Append(CameraMatrix[i, j] + "\t");
                }
                strRe.Append("\n");
            }
            DistCoeffs.ToList().ForEach(p => strRe.Append(p + "\n"));

            return strRe.ToString();
        }

        /// <summary>
        /// 控制台输出标定结果
        /// </summary>
        public void PrintCalibrateResultConsole()
        {
            for (int i = 0; i < CameraMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < CameraMatrix.GetLength(1); j++)
                {
                    Console.Write(CameraMatrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
            DistCoeffs.ToList().ForEach(p => Console.Write(p + "\t"));
            Console.WriteLine();
        }

        /// <summary>
        /// 读取标定结果本地文件
        /// </summary>
        /// <param name="path"></param>
        public void ReadDataFromTxt(string path)
        {
            var strs1 = File.ReadAllLines(path);
            for (int i = 0; i < strs1.Length - 1; i++)
            {
                var strs = strs1[i].Split(',');
                CameraMatrix[i, 0] = double.Parse(strs[0]);
                CameraMatrix[i, 1] = double.Parse(strs[1]);
                CameraMatrix[i, 2] = double.Parse(strs[2]);
            }
            for (int j = 0; j < 5; j++)
            {
                var strs = strs1.Last().Split(',');
                DistCoeffs[j] = double.Parse(strs[j]);
            }
            CameraMatrixMat = new Mat(3, 3, MatType.CV_64FC1, CameraMatrix);
            DiscoeffsMat = new Mat(1, 5, MatType.CV_64FC1, DistCoeffs);
        }

        /// <summary>
        /// 保存标定结果到txt
        /// </summary>
        /// <param name="name">txt文件名称</param>
        public void SaveData2Txt(string name)
        {
            List<string> datas = new List<string>();
            for (int i = 0; i < CameraMatrix.GetLength(0); i++)
            {
                var str = "";
                for (int j = 0; j < CameraMatrix.GetLength(1); j++)
                {
                    str += CameraMatrix[i, j] + ",";
                }
                datas.Add(str);
            }
            var str1 = "";
            DistCoeffs.ToList().ForEach(p => str1 += p + ",");
            datas.Add(str1);
            File.WriteAllLines(name + ".txt", datas);
        }

        public double[] Vec3d2doubleArray(Vec3d vec)
        {
            return new double[3] { vec.Item0, vec.Item1, vec.Item2 };
        }
    }
}