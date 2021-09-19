using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Numerics;

using UnmanageUtility;

namespace Client.Model.Service.ImageProcess
{
    public class GrayCodeProcess : IDisposable
    {
        private readonly Mat _rmat;
        private readonly Mat _tmat;
        private readonly Mat _camMat;
        private readonly Mat _projectorMat;
        private double[] _rvmatT;//投影仪与相机的关系矩阵
        private double[] _camMatrixInv;//相机内参矩阵的inv
        private double[] _projectorMatrixInv;//投影仪内参矩阵的inv
        private ResourcesTracker _resourcesTracker = new();
        UnmanagedArray<Point3f> _repts;
        public GrayCodeProcess(Mat rmat, Mat tmat, Mat camMat, Mat projectorMat)
        {
            _rmat = rmat;
            _tmat = tmat;
            _camMat = camMat;
            _projectorMat = projectorMat;
            Init();
        }

        public void Dispose()
        {
            _resourcesTracker.Dispose();
            _repts.Dispose();

        }

        public UnmanagedArray<Point3f> GetPointsAsync(List<Mat> mats, bool display = false)

        {
            Mat remat = _resourcesTracker.T(GrayCodePhaseShift.DeGrayCodeMatCal(mats, display));
            return get3d(remat);
        }

        private UnmanagedArray<Point3f> get3d(Mat matH)

        {
            _repts = new(matH.Rows * matH.Cols);

            Point3f rept = new();
            //var matHInd = matH.GetUnsafeGenericIndexer<short>();

            unsafe
            {
                Point3f* startPtr = (Point3f*)_repts.Ptr;
                short* matStartPtr = (short*)matH.DataStart;

                for (int y = 0; y < matH.Rows - 1; y++)
                {
                    for (int x = 0; x < matH.Cols; x++)
                    {
                        int ind = y * matH.Cols + x;
                        double[] xc3D = new double[3]
                            {
                            (_camMatrixInv[0]*x)+(_camMatrixInv[1]*y)+_camMatrixInv[2],
                            (_camMatrixInv[3]*x)+(_camMatrixInv[4]*y)+_camMatrixInv[5],
                            (_camMatrixInv[6]*x)+(_camMatrixInv[7]*y)+_camMatrixInv[8],
                            };
                        short* curMatInd = matStartPtr + ind;
                        short xp = *curMatInd;
                        //对应论文的xp，uv处的编码值
                        //_projectorMatrixInv对应论文的kp - 1
                        Vector3 x2V = new(
                            (float)((_projectorMatrixInv[1] * xp) + _projectorMatrixInv[2]),
                            (float)((_projectorMatrixInv[4] * xp) + _projectorMatrixInv[5]),
                            (float)((_projectorMatrixInv[7] * xp) + _projectorMatrixInv[8])
                            );//对应论文的x2
                        Vector3 x3V = new(
                            (float)((_projectorMatrixInv[0] * 1000) + (_projectorMatrixInv[1] * xp) + _projectorMatrixInv[2]),
                            (float)((_projectorMatrixInv[3] * 1000) + (_projectorMatrixInv[4] * xp) + _projectorMatrixInv[5]),
                            (float)((_projectorMatrixInv[6] * 1000) + (_projectorMatrixInv[7] * xp) + _projectorMatrixInv[8])
                            );//对应论文的x3
                        //var x2V = new Vector3(
                        //   (float)(_projectorMatrixInv[0] * xp + _projectorMatrixInv[1] * 0 + _projectorMatrixInv[2]),
                        //   (float)(_projectorMatrixInv[3] * xp + _projectorMatrixInv[4] * 0 + _projectorMatrixInv[5]),
                        //   (float)(_projectorMatrixInv[6] * xp + _projectorMatrixInv[7] * 0 + _projectorMatrixInv[8])
                        //   );//对应论文的x2
                        //var x3V = new Vector3(
                        //    (float)(_projectorMatrixInv[0] * xp + _projectorMatrixInv[1] * 500 + _projectorMatrixInv[2]),
                        //    (float)(_projectorMatrixInv[3] * xp + _projectorMatrixInv[4] * 500 + _projectorMatrixInv[5]),
                        //    (float)(_projectorMatrixInv[6] * xp + _projectorMatrixInv[7] * 500 + _projectorMatrixInv[8])
                        //    );//对应论文的x3

                        Vector3 x1V = new(0, 0, 0);

                        Vector3 ppV = Vector3.Cross(x1V - x3V, x2V - x3V);

                        double[] pc4d = new double[4]
                            {
                            (_rvmatT[0]*ppV.X)+(_rvmatT[1]*ppV.Y)+(_rvmatT[2]*ppV.Z)+_rvmatT[3],
                            (_rvmatT[4]*ppV.X)+(_rvmatT[5]*ppV.Y)+(_rvmatT[6]*ppV.Z)+_rvmatT[7],
                            (_rvmatT[8]*ppV.X)+(_rvmatT[9]*ppV.Y)+(_rvmatT[10]*ppV.Z)+_rvmatT[11],
                            (_rvmatT[12]*ppV.X)+(_rvmatT[13]*ppV.Y)+(_rvmatT[14]*ppV.Z)+_rvmatT[15]
                            };

                        double tmp1 = (pc4d[0] * xc3D[0]) + (pc4d[1] * xc3D[1]) + (pc4d[2] * xc3D[2]);
                        double x1 = -pc4d[3] * xc3D[0] / tmp1;
                        double y1 = -pc4d[3] * xc3D[1] / tmp1;

                        double z1 = (-pc4d[3]) / tmp1;

                        rept = new Point3f((float)x1, (float)y1, (float)z1);

                        Point3f* curP = startPtr + ind;

                        *curP = rept;
                        //repts[ind] = rept;
                    }
                }
            }

            return _repts;
        }

        private void Init()
        {
            _rmat.GetArray<double>(out var vs11);
            _tmat.GetArray<double>(out var vs1);
            double[,] rvtmp1 = new double[4, 4]
                    {
                    { vs11[0],vs11[1],vs11[2],vs1[0] },
                    { vs11[3],vs11[4],vs11[5],vs1[1] },
                    { vs11[6],vs11[7],vs11[8],vs1[2] },
                    {0,0,0,1 }
                    };
            Mat rvmat1 = _resourcesTracker.T(new Mat(4, 4, MatType.CV_64FC1, rvtmp1));
            rvmat1.T().ToMat().GetArray<double>(out _rvmatT);
            _camMat.Inv().ToMat().GetArray<double>(out _camMatrixInv);
            _projectorMat.Inv().ToMat().GetArray<double>(out _projectorMatrixInv);
        }
    }
}