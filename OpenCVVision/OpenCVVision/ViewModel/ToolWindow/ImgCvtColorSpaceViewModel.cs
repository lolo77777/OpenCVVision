using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Event;
using OpenCVVision.Model.Interface.Tool;
using OpenCVVision.Model.Tools.ImgPreProcess;
using Stylet;

namespace OpenCVVision.ViewModel.ToolWindow
{
    public class ImgCvtColorSpaceViewModel : Screen
    {
        private IEventAggregator eventAggregator;
        private Dictionary<string, int> ImgChanelItemsvalue = new Dictionary<string, int>();
        private ImgCvtColorSpace ImgCvtColorSpace;

        public ImgCvtColorSpaceViewModel(IEventAggregator _eventAggregator)
        {
            eventAggregator = _eventAggregator;
            ImgCvtColorSpace = new ImgCvtColorSpace(eventAggregator);
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
                ImgChanelItems = new BindableCollection<int>() { 0, 1, 2, 3 };
            }
            else
            {
                ImgChanelItems = new BindableCollection<int>() { 0, 1, 2 };
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
        }

        #region Action

        public void Run()
        {
            ImgCvtColorSpace.OutputMat = ImgCvtColorSpace.Run(ImgCvtColorSpace.InputMat, (ImgSpace)(ImgChanelItemsvalue[ImgSapceStr]), ImgChanelInt);
            eventAggregator.Publish(new DisImgEvent(ImgCvtColorSpace.OutputMat));
        }

        public void Cancle()
        {
            ImgCvtColorSpace.Cancle();
            this.RequestClose();
        }

        #endregion Action
    }
}