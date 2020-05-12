using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using OpenCVVision.Model.Event;
using OpenCVVision.Model.Tools.ImgPreProcess;
using Stylet;
using OpenCvSharp;
using System.Linq;
using OpenCVVision.Model.Interface;

namespace OpenCVVision.ViewModel.ToolWindow
{
    public class ImgThresholdViewModel : Screen, IToolWindow
    {
        private ImgThreshold ImgTool;
        private IEventAggregator eventAggregator;
        private Dictionary<string,int> toolTypeDic = new Dictionary<string,int>();
        private IOperaHistory operaHistory;
        private bool savestatus;

        public ImgThresholdViewModel(IEventAggregator _eventAggregator,IOperaHistory _operaHistory)
        {
            eventAggregator = _eventAggregator;
            ImgTool = new ImgThreshold(eventAggregator);
            operaHistory = _operaHistory;
            init();
        }

        #region BingProperty

        public BindableCollection<string> ToolTypeItems { get; set; } = new BindableCollection<string>();
        public string ToolTypeStr { get; set; }
        public int Thresh { get; set; }
        public int SetValue { get; set; } = 255;

        #endregion BingProperty

        #region Action

        public void Run()
        {
            ImgTool.OutputMat = ImgTool.Run(ImgTool.InputMat,Thresh,SetValue,(ThresholdTypes)toolTypeDic[ToolTypeStr]);
            eventAggregator.Publish(new DisImgEvent(ImgTool.OutputMat));
            savestatus = true;
        }

        public void Cancle()
        {
            ImgTool.Cancle();
            savestatus = false;
            this.RequestClose();
        }

        public void Restore()
        {
            ImgTool.Restore();
            savestatus = false;
        }

        #endregion Action

        #region PrivateFunction

        private void init()
        {
            ToolTypeItems.Clear();
            toolTypeDic.Clear();
            foreach (var type in Enum.GetValues(typeof(ThresholdTypes)))
            {
                ToolTypeItems.Add(type.ToString());
                toolTypeDic[type.ToString()] = (int)type;
            }
            ToolTypeStr = ToolTypeItems.First();
        }

        #endregion PrivateFunction

        protected override void OnClose()
        {
            if (savestatus)
            {
                operaHistory.Record.Add((ImgTool.Name, ImgTool.OutputMat));
                eventAggregator.Publish(new UpdateRecord());
            }
            base.OnClose();
        }
    }
}