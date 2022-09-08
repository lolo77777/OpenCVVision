using System.Text;

namespace Client.Services;

public class PaddleXInference : IDisposable, IEnableLogger
{
    private readonly byte[] _paddlex_model_type = new byte[10];  // det/seg/clas + \0
    private bool _isInfer = false;
    private Scalar[] _scalars;
    private bool _disposedValue;

    public string ModelType { get; private set; } = string.Empty;

    public PaddleXInference()
    {
        _scalars = new Scalar[] { Scalar.Red, Scalar.Green, Scalar.Blue, Scalar.PaleGoldenrod, Scalar.Orange, Scalar.Pink, Scalar.PowderBlue };
    }

    public Result DestroyModel()
    {
        return Result.Try(() =>
        {
            NativeMethod.DestructModel();
        });
    }

    public Result<(List<(string type, Rect Rect)> targetRect, Mat reMat, int ms)> InferImgDet(Mat src)
    {
        if (!_isInfer)
        {
            return DetInfer(src);
        }
        else
        {
            return Result.Fail("infer:正在识别中。。");
        }
    }

    public Result<(string reStr, Mat reMat)> InferImgCls(Mat src)
    {
        if (!_isInfer)
        {
            return ClsInfer(src);
        }
        else
        {
            return Result.Fail("infer:正在识别中。。");
        }
    }

    private byte[] GetColorMapList(int num_classes = 256)
    {
        num_classes += 1;
        byte[] color_map = new byte[num_classes * 3];
        for (int i = 0; i < num_classes; i++)
        {
            int j = 0;
            int lab = i;
            while (lab != 0)
            {
                color_map[i * 3] |= (byte)(((lab >> 0) & 1) << (7 - j));
                color_map[i * 3 + 1] |= (byte)(((lab >> 1) & 1) << (7 - j));
                color_map[i * 3 + 2] |= (byte)(((lab >> 2) & 1) << (7 - j));

                j += 1;
                lab >>= 3;
            }
        }

        color_map = color_map.Skip(3).ToArray();

        return color_map;
    }

    private static IntPtr FloatToIntptr(float[] bytes)
    {
        GCHandle hObject = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        return hObject.AddrOfPinnedObject();
    }

    private Result<(List<(string type, Rect Rect)> targetRect, Mat reMat, int ms)> DetInfer(Mat src)
    {
        var reMat = src.Clone();
        List<(string type, Rect Rect)> reList = new();
        try
        {
            _isInfer = true;
            byte[] color_map = GetColorMapList(256);
            var inputData = GetBGRValues(src.Clone());
            float[] resultlist = new float[600];
            IntPtr results = FloatToIntptr(resultlist);
            int[] boxesInfo = new int[1]; // 10 boundingbox
            byte[] labellist = new byte[1000];

            TimeSpan infer_start_time = new TimeSpan(DateTime.Now.Ticks);
            // The fourth parameter is the number of channels for inputting images
            NativeMethod.Det_ModelPredict(inputData, src.Width, src.Height, 3, results, boxesInfo, ref labellist[0]);
            TimeSpan infer_end_time = new(DateTime.Now.Ticks);

            string strGet = Encoding.Default.GetString(labellist, 0, labellist.Length);
            string[] predict_Label_List = strGet.Split(' ');

            for (int i = 0; i < boxesInfo[0]; i++)
            {
                int labelindex = Convert.ToInt32(resultlist[i * 6 + 0]);
                float score = resultlist[i * 6 + 1];
                float left = resultlist[i * 6 + 2];
                float top = resultlist[i * 6 + 3];
                float right = resultlist[i * 6 + 4];
                float down = resultlist[i * 6 + 5];
                if (score >= 0.6)
                {
                    var text = $"{predict_Label_List[i]}-{labelindex}-: {score:f2}";
                    var text_size = Cv2.GetTextSize(text, HersheyFonts.HersheySimplex, 1, 2, out int baseline);
                    int left_down_x = (int)left + 22;
                    int left_down_y = (int)top + text_size.Height;

                    var rectTmp = new Rect((int)left, (int)top, (int)right, (int)down);

                    Cv2.Rectangle(reMat, rectTmp, _scalars[labelindex], 2, LineTypes.AntiAlias);//LineTypes.AntiAlias:反锯齿效果
                    Cv2.PutText(reMat, text, new OpenCvSharp.Point(left_down_x, left_down_y), HersheyFonts.HersheySimplex, 1, _scalars[labelindex], 2, LineTypes.Link4);
                    reList.Add((text, rectTmp));
                }
            }
            TimeSpan start2end_time = infer_end_time.Subtract(infer_start_time).Duration();
            var costMilliseconds = (int)start2end_time.TotalMilliseconds;
            _isInfer = false;
            return Result.Ok((reList, reMat, costMilliseconds));
        }
        catch (Exception ex)
        {
            _isInfer = false;
            return Result.Fail(ex.Message);
        }
    }

    private Result<(string reStr, Mat reMat)> ClsInfer(Mat src)
    {
        try
        {
            _isInfer = true;
            TimeSpan infer_start_time = new(DateTime.Now.Ticks);

            Cv2.Resize(src, src, new OpenCvSharp.Size(128, 128));

            var inputData = GetBGRValues(src.Clone());

            float[]? pre_score = new float[1];
            int[]? pre_category_id = new int[1];
            byte[]? pre_category = new byte[200];

            NativeMethod.Cls_ModelPredict(inputData, src.Width, src.Height, 3, ref pre_score[0], ref pre_category[0], ref pre_category_id[0]);
            TimeSpan infer_end_time = new(DateTime.Now.Ticks);

            string category_strGet = Encoding.Default.GetString(pre_category, 0, pre_category.Length).Split('\0')[0];

            TimeSpan start2end_time = infer_end_time.Subtract(infer_start_time).Duration();
            double cost_milliseconds = start2end_time.TotalMilliseconds;
            var reStr = $"推测结果:{category_strGet},分数:{pre_score[0]},时间:{cost_milliseconds}ms";

            inputData = null;
            pre_score = null;
            pre_category_id = null;
            pre_category = null;
            _isInfer = false;

            return Result.Ok((reStr, src));
        }
        catch (Exception ex)
        {
            _isInfer = false;
            return Result.Fail(ex.Message);
        }
    }

    private static byte[] GetBGRValues(Mat src)
    {
        var splits = src.Split();
        splits[0].GetArray<byte>(out var vs0);
        splits[1].GetArray<byte>(out var vs1);
        splits[2].GetArray<byte>(out var vs2);
        var inputData = new byte[vs0.Length * 3];
        for (int i = 0; i < vs0.Length; i++)
        {
            inputData[3 * i] = vs0[i];
            inputData[3 * i + 1] = vs1[i];
            inputData[3 * i + 2] = vs2[i];
        }
        splits.ToList().ForEach(s => s.Dispose());
        src.Dispose();
        return inputData;
    }

    public Result LoadModel(string folder)
    {
        return Result.Try(() =>
        {
            ModelType = "paddlex";
            var model_filename = Path.Combine(folder, "model.pdmodel");
            var params_filename = Path.Combine(folder, "model.pdiparams");
            var cfg_file = Path.Combine(folder, "model.yml");
            var use_gpu = false;
            var gpu_id = 0;
            NativeMethod.InitModel(ModelType, model_filename, params_filename, cfg_file, use_gpu, gpu_id, ref _paddlex_model_type[0]);
            ModelType = Encoding.UTF8.GetString(_paddlex_model_type).Split('\0')[0];
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                try
                {
                    NativeMethod.DestructModel();
                    this.Log().Write("模型已释放", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    this.Log().Write(ex.Message, LogLevel.Warn);
                }
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null

            _disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器 ~PaddleInfer() { //
    // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中 Dispose(disposing: false); }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

internal static class NativeMethod
{
    /**********************************************************************/
    /*****************          1.Reasoning DLL import implementation          ****************/
    /**********************************************************************/

    // Load inference correlation methods
    [DllImport("model_infer.dll", EntryPoint = "InitModel")] // Model unified initialization method: need yml、pdmodel、pdiparams
    public static extern void InitModel(string model_type, string model_filename, string params_filename, string cfg_file, bool use_gpu, int gpu_id, ref byte paddlex_model_type);

    [DllImport("model_infer.dll", EntryPoint = "Det_ModelPredict")]  // PaddleDetection Model reasoning method
    public static extern void Det_ModelPredict(byte[] img, int W, int H, int C, IntPtr output, int[] BoxesNum, ref byte label);

    [DllImport("model_infer.dll", EntryPoint = "Seg_ModelPredict")]  // PaddleSeg Model reasoning method
    public static extern void Seg_ModelPredict(byte[] img, int W, int H, int C, ref byte output);

    [DllImport("model_infer.dll", EntryPoint = "Cls_ModelPredict")]  // PaddleClas Model reasoning method
    public static extern void Cls_ModelPredict(byte[] img, int W, int H, int C, ref float score, ref byte category, ref int category_id);

    [DllImport("model_infer.dll", EntryPoint = "Mask_ModelPredict")]  // Paddlex-MaskRCNN Model reasoning method
    public static extern void Mask_ModelPredict(byte[] img, int W, int H, int C, IntPtr output, ref byte Mask_output, int[] BoxesNum, ref byte label);

    [DllImport("model_infer.dll", EntryPoint = "DestructModel")]  // Segmentation, detection, identification model destruction method
    public static extern void DestructModel();
}