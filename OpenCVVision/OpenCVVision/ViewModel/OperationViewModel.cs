using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Event;
using OpenCVVision.Model.Interface.Tool;
using OpenCVVision.ViewModel.ToolWindow;
using Stylet;

namespace OpenCVVision.ViewModel
{
    public class OperationViewModel : Screen
    {
        private string[] loadImgSams;
        private IEventAggregator eventAggregator;
        private IWindowManager windowManager;

        public OperationViewModel(IEventAggregator eventAggregator, IWindowManager _windowManager)
        {
            this.eventAggregator = eventAggregator;
            this.windowManager = _windowManager;
        }

        public BindableCollection<ToolName> LoadImgTools { get; set; }

        private void init()
        {
            loadImgSams = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(ILoadImg)))
                 .Select(x => x.Name)
                 .ToArray();

            LoadImgTools = new BindableCollection<ToolName>((from p in loadImgSams
                                                             select new ToolName(p)).ToList());
        }

        public void ImgCvtImgSpace()
        {
        }

        public void ImgBlur()
        {
        }

        public void ToolsRun(object sender, EventArgs eventArgs)
        {
            //Select(x => (ILoadImg)Activator.CreateInstance(x))

            var btn = (Button)sender;
            if (btn.Content.ToString().Contains("滤波"))
            {
                windowManager.ShowDialog(new ImgBlurViewModel(eventAggregator));
            }
            else if (btn.Content.ToString().Contains("图像空间"))
            {
                windowManager.ShowDialog(new ImgCvtColorSpaceViewModel(eventAggregator)); ;
            }
            else if (btn.Content.ToString().Contains("加载") || btn.Content.ToString().Contains("Load"))
            {
                using var loadImgSam = (Assembly.GetEntryAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(ILoadImg)))
                .Where(t => t.Name.Equals(btn.Content))
                .Select(x => (ILoadImg)Activator.CreateInstance(x))).First();
                var src = loadImgSam.Run();

                eventAggregator.Publish(new DisImgEvent(src));
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            init();
        }
    }

    public class ToolName
    {
        public ToolName(string _name)
        {
            Name = _name;
        }

        public string Name { get; set; }
    }
}