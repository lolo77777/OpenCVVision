using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Event;
using OpenCVVision.Model.Interface;
using OpenCVVision.Model.Interface.Tool;
using OpenCVVision.Model.Tools.ImgPreProcess;
using Stylet;

namespace OpenCVVision.ViewModel.ToolWindow
{
    public class ImgCvtColorSpaceViewModel : Screen
    {
        private IEventAggregator eventAggregator;
        private Dictionary<string,int> ImgChanelItemsvalue = new Dictionary<string,int>();
        private ImgCvtColorSpace ImgCvtColorSpace;
        private IOperaHistory operaHistory;
        private bool savestatus = false;

        public ImgCvtColorSpaceViewModel(IEventAggregator _eventAggregator,IOperaHistory _operaHistory)
        {
            eventAggregator = _eventAggregator;
            ImgCvtColorSpace = new ImgCvtColorSpace(eventAggregator);
            operaHistory = _operaHistory;

            init();
        }

        #region BindProperty

        public BindableCollection<string> ImgColorSapceItems { get; set; } = new BindableCollection<string>();
        public BindableCollection<int> ImgChanelItems { get; set; } = new BindableCollection<int>();

        private string _imgSapceStr;

        public string ImgSapceStr
        {
            get => _imgSapceStr;

            set
            {
                _imgSapceStr = value;
                ImgChanelIntChange();
                ImgChanelInt = 0;
                NotifyOfPropertyChange(() => ImgSapceStr);
            }
        }

        public int ImgChanelInt { set; get; } = 0;

        #endregion BindProperty

        private void ImgChanelIntChange()
        {
            if (ImgChanelItemsvalue[ImgSapceStr] == 0)
            {
                ImgChanelItems = new BindableCollection<int>() { 0 };
            }
            else if (ImgSapceStr == "BGRA")
            {
                ImgChanelItems = new BindableCollection<int>() { 0,1,2,3 };
            }
            else
            {
                ImgChanelItems = new BindableCollection<int>() { 0,1,2 };
            }
        }

        private void init()
        {
            ImgColorSapceItems.Clear();
            ImgChanelItemsvalue.Clear();
            foreach (var en in Enum.GetValues(typeof(ImgSpace)))
            {
                ImgColorSapceItems.Add(en.ToString());
                ImgChanelItemsvalue[en.ToString()] = Convert.ToInt32(en);
            }
            ImgSapceStr = ImgColorSapceItems.First();
        }

        #region Action

        public void Run()
        {
            ImgCvtColorSpace.OutputMat = ImgCvtColorSpace.Run(ImgCvtColorSpace.InputMat,(ImgSpace)(ImgChanelItemsvalue[ImgSapceStr]),ImgChanelInt);
            savestatus = true;
            eventAggregator.Publish(new DisImgEvent(ImgCvtColorSpace.OutputMat));
        }

        public void Cancle()
        {
            savestatus = false;
            ImgCvtColorSpace.Cancle();
            this.RequestClose();
        }

        public void Restore()
        {
            savestatus = false;
            ImgCvtColorSpace.Restore();
        }

        #endregion Action

        protected override void OnClose()
        {
            if (savestatus)
            {
                operaHistory.Record.Add((ImgCvtColorSpace.Name, ImgCvtColorSpace.OutputMat));
                eventAggregator.Publish(new UpdateRecord());
            }
            base.OnClose();
        }
    }
}