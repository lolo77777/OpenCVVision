using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using OpenCVVision.Model.Event;
using OpenCVVision.Model.Interface;
using Stylet;

namespace OpenCVVision.ViewModel
{
    public class OperationHistoryViewModel : Screen, IHandle<UpdateRecord>
    {
        private IOperaHistory operaHistory;
        private IEventAggregator eventAggregator;

        public OperationHistoryViewModel(IOperaHistory history,IEventAggregator _eventAggregator)
        {
            operaHistory = history;
            eventAggregator = _eventAggregator;
            eventAggregator.Subscribe(this);
        }

        public BindableCollection<RecordEleViewModel> OperaHistoryRecordItems { get; set; } = new BindableCollection<RecordEleViewModel>();
        private int _recordSelect;

        public int RecordSelect
        {
            get => _recordSelect;
            set
            {
                _recordSelect = value;
                updateUI();
                NotifyOfPropertyChange(() => RecordSelect);
            }
        }

        /// <summary>
        /// 元素列表发生了变化，响应动作
        /// </summary>
        /// <param name="message"></param>
        public void Handle(UpdateRecord message)
        {
            RecordSelect = operaHistory.Record.Count - 1;
            initItems();
            updateUI();
        }

        /// <summary>
        /// 更新图像窗口的显示
        /// </summary>
        private void updateUI()
        {
            eventAggregator.Publish(new DisImgEvent(operaHistory.Record[RecordSelect].OutputMat));
        }

        /// <summary>
        /// 初始化整个列表
        /// </summary>
        private void initItems()
        {
            var tmplist = new List<RecordEleViewModel>();
            foreach (var record in operaHistory.Record)
            {
                tmplist.Add(new RecordEleViewModel(record.Name,record.OutputMat));
            }
            OperaHistoryRecordItems = new BindableCollection<RecordEleViewModel>(tmplist);
        }
    }
}