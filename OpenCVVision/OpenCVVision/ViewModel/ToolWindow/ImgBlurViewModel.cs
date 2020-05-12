using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Event;
using OpenCVVision.Model.Interface;
using OpenCVVision.Model.Tools.ImgPreProcess;
using Stylet;

namespace OpenCVVision.ViewModel.ToolWindow
{
    public class ImgBlurViewModel : Screen
    {
        private ImgBlur ImgBlur;
        private Dictionary<string,int> imgblurDic = new Dictionary<string,int>();
        private IEventAggregator eventAggregator;
        private IOperaHistory operaHistory;
        private Mat tmpMat = new Mat();
        private bool savestaus = false;

        public ImgBlurViewModel(IEventAggregator _eventAggregator,IOperaHistory _operaHistory)
        {
            eventAggregator = _eventAggregator;
            ImgBlur = new ImgBlur(eventAggregator);
            operaHistory = _operaHistory;
            init();
        }

        #region BindProperty

        public BindableCollection<string> ToolTypeItems { get; set; } = new BindableCollection<string>();
        public BindableCollection<int> ImgKSizeItems { get; set; } = new BindableCollection<int> { 3,5,7,9,11 };
        private int _runCount;

        public int RunCount
        {
            get => _runCount;
            set
            {
                _runCount = value;
                RunCountLabel = "迭代次数：" + value;
                NotifyOfPropertyChange(() => RunCount);
            }
        }

        private int _sigmaX;
        private int _sigmaY;

        public int SigmaX
        {
            get => _sigmaX;
            set
            {
                _sigmaX = value;
                SigmaXLabel = "sigmaX:" + value / 100f;
                NotifyOfPropertyChange(() => SigmaX);
            }
        }

        public int SigmaY
        {
            get => _sigmaY;
            set
            {
                _sigmaY = value;
                SigmaYLabel = "sigmaY:" + value / 100f;
                NotifyOfPropertyChange(() => SigmaY);
            }
        }

        public int ImgKSize { get; set; }
        public string RunCountLabel { get; set; } = "迭代次数:1";
        public string SigmaXLabel { get; set; } = "sigmaX:0";
        public string SigmaYLabel { get; set; } = "sigmaY:0";
        public bool IsGauss { get; set; } = false;
        private string _toolTypeStr;

        public string ToolTypeStr
        {
            get => _toolTypeStr;
            set
            {
                _toolTypeStr = value;
                if (value.Contains("Gauss"))
                {
                    IsGauss = true;
                }
                else
                {
                    IsGauss = false;
                }
                NotifyOfPropertyChange(() => ToolTypeStr);
            }
        }

        #endregion BindProperty

        #region Action

        public void Run()
        {
            ImgBlur.OutputMat = ImgBlur.Run(ImgBlur.InputMat,(BlurMethod)imgblurDic[ToolTypeStr],ImgKSize,RunCount,SigmaX / 100f,SigmaY / 100f);
            savestaus = true;
            eventAggregator.Publish(new DisImgEvent(ImgBlur.OutputMat));
        }

        public void Cancle()
        {
            ImgBlur.Cancle();
            savestaus = false;
            RequestClose();
        }

        public void Restore()
        {
            ImgBlur.Restore();
            savestaus = false;
        }

        protected override void OnClose()
        {
            if (savestaus)
            {
                operaHistory.Record.Add((ImgBlur.Name, ImgBlur.OutputMat));
                eventAggregator.Publish(new UpdateRecord());
            }
            base.OnClose();
        }

        #endregion Action

        private void init()
        {
            ToolTypeItems.Clear();
            imgblurDic.Clear();
            foreach (var blurmethod in Enum.GetValues(typeof(BlurMethod)))
            {
                ToolTypeItems.Add(blurmethod.ToString());
                imgblurDic[blurmethod.ToString()] = (int)blurmethod;
            }
            ToolTypeStr = ToolTypeItems.First();
            ImgKSize = ImgKSizeItems.First();
        }
    }
}