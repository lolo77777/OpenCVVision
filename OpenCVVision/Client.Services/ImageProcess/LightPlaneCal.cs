namespace Client.Services.ImageProcess;

public class LightPlaneCal : IDisposable
{
    private ResourcesTracker _resourcesTracker = new();

    /// <summary>
    /// 通过光平面系数将像素坐标转换为深度坐标
    /// </summary>
    /// <param name="point2Ds"></param>
    /// <param name="src"></param>
    /// <param name="lightPlaneCoefficient"></param>
    /// <returns></returns>
    private IList<Point3d> CalFrameToCameraWithLightPlane(IEnumerable<Point2d> point2Ds, Mat src, Mat lightPlaneCoefficient)
    {
        List<Point3d> rePt3Fs = new();
        Mat camMatInv = _resourcesTracker.T(src.Inv().ToMat());
        camMatInv.GetArray<double>(out var camMatrixInv);
        lightPlaneCoefficient.GetArray<float>(out var coefficient);
        float a = coefficient[0]; float b = coefficient[1]; float c = coefficient[2];
        foreach (Point2d pt2d in point2Ds)
        {
            Point3d pt3FTmp = new((camMatrixInv[0] * pt2d.X) + camMatrixInv[2], (camMatrixInv[4] * pt2d.Y) + camMatrixInv[5], 1);
            double xpp = pt3FTmp.X;
            double ypp = pt3FTmp.Y;
            double x = c * xpp / (1 - (a * xpp) - (b * ypp));
            double y = c * ypp / (1 - (a * xpp) - (b * ypp));
            double z = (a * x) + (b * y) + c;
            Point3d pt3F = new(x, y, z);
            rePt3Fs.Add(pt3F);
        }
        return rePt3Fs;
    }

    /// <summary>
    /// 灰度质心提取光条中心
    /// </summary>
    /// <param name="roi">光条所在区域</param>
    /// <param name="rowstart">起始行数</param>
    /// <param name="display">是否使用CV窗口显示</param>
    /// <returns></returns>
    private (Point2d[] laserPoint2f, Point[] laserPoint, Mat mat) GetLine(Mat roi, int rowstart, bool display = false)
    {
        Point2d[] relist2F = new Point2d[roi.Width];
        Point[] relist = new Point[roi.Width];
        Mat.UnsafeIndexer<byte> roiindex = roi.GetUnsafeGenericIndexer<byte>();

        for (int x = 0; x < roi.Cols; x++)
        {
            roi.Col(x).MinMaxLoc(out _, out double maxx, out _, out Point maxloc);
            float u = 0, g = 0;
            float location;
            if (maxx > 50)
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

        Mat remat = _resourcesTracker.T(roi.CvtColor(ColorConversionCodes.GRAY2BGR));
        Vec3b color = new(0, 0, 250);
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

    /// <summary>
    /// 获取深度坐标与曲线图像
    /// </summary>
    /// <param name="src">原始图像</param>
    /// <param name="templete">光源模板</param>
    /// <param name="rvMat">外参矩阵</param>
    /// <param name="cameraMatrixMat">内参矩阵</param>
    /// <param name="lightPlaneCoeffient">光平面系数</param>
    /// <returns></returns>
    public (Point2d[] pt2fs, Mat thinMat) GetResultGray(Mat src, Mat templete, Mat rvMat, Mat cameraMatrixMat, Mat lightPlaneCoeffient)
    {
        (Point2d[] laserPoint2F, _, Mat mat) = GetLine(src, 0);
        List<Point3d> pt3Fs = CalFrameToCameraWithLightPlane(laserPoint2F, cameraMatrixMat, lightPlaneCoeffient).ToList();

        Point2d[] pt2Fs = new Point2d[pt3Fs.Count];

        for (int i = 0; i < pt3Fs.Count; i++)
        {
            Point3d ptmp = pt3Fs[i];
            MatExpr mtmp = _resourcesTracker.T(rvMat * new Mat(3, 1, MatType.CV_64FC1, new double[3] { ptmp.X, ptmp.Y, ptmp.Z }));
            mtmp.ToMat().GetArray<double>(out var vs1);
            pt2Fs[i] = new Point2d(vs1[0], vs1[2]);
        }
        pt3Fs.Clear();
        return (pt2Fs, mat);
    }

    public void Dispose()
    {
        _resourcesTracker.Dispose();
    }
}