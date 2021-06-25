using Client.Data;

using OpenCvSharp;
using OpenCvSharp.Dnn;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Client.ViewModel.Operation
{
    /// <summary>
    /// 参考引用了以下项目、资料 https://github.com/died/YOLO3-With-OpenCvSharp4 https://pjreddie.com/darknet/yolo/
    /// </summary>
    [OperationInfo(12.1, "YoloV4", MaterialDesignThemes.Wpf.PackIconKind.Yoga)]
    public class YoloViewModel : OperaViewModelBase
    {
        private readonly Scalar[] _colors = Enumerable.Repeat(false, 80).Select(x => Scalar.RandomColor()).ToArray();
        private bool _isInit;
        private string[] _labels;
        private readonly Interaction<Unit, string> _loadFileConfirm = new();
        private Net _net;
        private List<Mat> _srcs;
        public Interaction<Unit, string> LoadFileConfirm => _loadFileConfirm;
        public ReactiveCommand<string, Unit> LoadImageCommand { get; set; }

        [Reactive] public string TxtImageFilePath { get; set; }

        [Reactive] public string TxtCfgFilePath { get; set; }

        protected override void SetupCommands()
        {
            base.SetupCommands();
            LoadImageCommand = ReactiveCommand.Create<string>(LoadFile);
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
            _imageDataManager.InputMatGuidSubject
                .Where(guid => CanOperat && _isInit)
                .Do(guid => UpdateOutput())
                .Subscribe()
                .DisposeWith(d);
            this.WhenAnyValue(x => x.TxtImageFilePath, x => x.TxtCfgFilePath)
                .Where(vt => !string.IsNullOrWhiteSpace(vt.Item1) && !string.IsNullOrWhiteSpace(vt.Item2))

                .Subscribe(vt => InitNet(vt.Item1, vt.Item2))
                .DisposeWith(d);
        }

        #region PrivateFunction

        /// <summary>
        /// Draw result to image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="classes"></param>
        /// <param name="confidence"></param>
        /// <param name="probability"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void Draw(Mat image, int classes, float confidence, float probability, double centerX, double centerY, double width, double height)
        {
            //label formating
            var label = $"{_labels[classes]} {probability * 100:0.00}%";
            //Console.WriteLine($"confidence {confidence * 100:0.00}% {label}");
            var x1 = (centerX - width / 2) < 0 ? 0 : centerX - width / 2; //avoid left side over edge
            //draw result
            image.Rectangle(new Point(x1, centerY - height / 2), new Point(centerX + width / 2, centerY + height / 2), _colors[classes], 2);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.5, 1, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, centerY - height / 2 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), _colors[classes], Cv2.FILLED);
            var textColor = Cv2.Mean(_colors[classes]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, centerY - height / 2 - baseline), HersheyFonts.HersheyTriplex, 0.5, textColor);
        }

        /// <summary>
        /// Get result form all output
        /// </summary>
        /// <param name="output"></param>
        /// <param name="image"></param>
        /// <param name="threshold"></param>
        /// <param name="nmsThreshold">threshold for nms</param>
        /// <param name="nms">Enable Non-maximum suppression or not</param>
        private void GetResult(IEnumerable<Mat> output, Mat image, float threshold, float nmsThreshold, bool nms = true)
        {
            //for nms
            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
            var boxes = new List<Rect2d>();

            var w = image.Width;
            var h = image.Height;
            /*
             YOLO3 COCO trainval output
             0 1 : center                    2 3 : w/h
             4 : confidence                  5 ~ 84 : class probability
            */
            const int prefix = 5;   //skip 0~4

            foreach (var prob in output)
            {
                var probindx = prob.GetUnsafeGenericIndexer<float>();

                for (var y = 0; y < prob.Rows; y++)
                {
                    var confidence = probindx[y, 4];
                    if (confidence > threshold)
                    {
                        //get classes probability

                        Cv2.MinMaxLoc(prob.Row(y).ColRange(prefix, prob.Cols), out _, out Point max);
                        var classes = max.X;
                        var probability = probindx[y, classes + prefix];

                        if (probability > threshold) //more accuracy, you can cancel it
                        {
                            //get center and width/height
                            var centerX = probindx[y, 0] * w;
                            var centerY = probindx[y, 1] * h;
                            var width = probindx[y, 2] * w;
                            var height = probindx[y, 3] * h;

                            if (!nms)
                            {
                                // draw result (if don't use NMSBoxes)
                                Draw(image, classes, confidence, probability, centerX, centerY, width, height);
                                continue;
                            }

                            //put data to list for NMSBoxes
                            classIds.Add(classes);
                            confidences.Add(confidence);
                            probabilities.Add(probability);
                            boxes.Add(new Rect2d(centerX, centerY, width, height));
                        }
                    }
                }
            }

            if (!nms) return;

            //using non-maximum suppression to reduce overlapping low confidence box
            CvDnn.NMSBoxes(boxes, confidences, threshold, nmsThreshold, out int[] indices);

            //Console.WriteLine($"NMSBoxes drop {confidences.Count - indices.Length} overlapping result.");

            foreach (var i in indices)
            {
                var box = boxes[i];
                Draw(image, classIds[i], confidences[i], probabilities[i], box.X, box.Y, box.Width, box.Height);
            }
        }

        private void InitNet(string filepath, string cfgFilePath)
        {
            _net = CvDnn.ReadNetFromDarknet(cfgFilePath, filepath);
            _labels = File.ReadAllLines(FilePath.File.ObjectNames).ToArray();
            _net.SetPreferableBackend(Backend.OPENCV);
            _net.SetPreferableTarget(Target.CPU);
            _srcs = Directory.GetFiles(FilePath.Folder.YoloV3TestImage).Select(str => Cv2.ImRead(str)).ToList();
            for (int i = 0; i < _srcs.Count; i++)
            {
                var txtMark = $"YoloV3Test{i}";
                if (!_imageDataManager.IsExsitByMark(txtMark))
                {
                    _imageDataManager.AddImage($"YoloV3Test{i}", _srcs[i]);
                }
            }
            _isInit = true;
        }

        private void LoadFile(string name)
        {
            _loadFileConfirm.Handle(Unit.Default)
                .Where(str => str.Contains(".cfg") || str.Contains(".weight"))
                .Subscribe(str =>
                {
                    if (name.Equals("weight"))
                    {
                        TxtImageFilePath = str;
                    }
                    else
                    {
                        TxtCfgFilePath = str;
                    }
                });
        }

        private void UpdateOutput()
        {
            SendTime(() =>
            {
                var blogimg = CvDnn.BlobFromImage(_src, 1 / 255.0, new Size(416, 416), new Scalar(), true, false);
                _net.SetInput(blogimg);
                var names = _net.GetUnconnectedOutLayersNames();
                var outputMats = names.Select(_ => new Mat()).ToArray();
                _net.Forward(outputMats, names);
                GetResult(outputMats, _src, 0.5f, 0.1f, true);
                _imageDataManager.OutputMatSubject.OnNext(_src.Clone());
            });
        }

        #endregion PrivateFunction
    }
}