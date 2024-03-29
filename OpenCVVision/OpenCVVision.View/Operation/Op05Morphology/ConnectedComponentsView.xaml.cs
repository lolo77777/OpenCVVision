﻿namespace OpenCVVision.View.Operation;

/// <summary>
/// ConnectedComponentsView.xaml 的交互逻辑
/// </summary>
public partial class ConnectedComponentsView
{
    public ConnectedComponentsView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.AreaLimit, v => v.sliderAreaMax.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderHeightMax.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderWidthMax.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.AreaLimit, v => v.sliderAreaMin.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderHeightMin.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderWidthMin.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderTopMax.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderTopMin.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderLeftMax.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderLeftMin.Maximum).DisposeWith(d);
            //this.WhenAnyValue(x => x.FilterList.SelectedItems)
            //    .Select(t => (IList<object>)FilterList.SelectedItems)
            //    .Where(listtmp => listtmp.Count > 0)
            //    .Select(listtmp => listtmp.Select(t => ((ListBoxItem)t).Content.ToString()).ToList())
            //    .BindTo(ViewModel, x => x.Filters)
            //    .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderAreaMax.Value)
                .BindTo(ViewModel, x => x.AreaMax)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderAreaMin.Value)
                .BindTo(ViewModel, x => x.AreaMin)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderHeightMax.Value)
                .BindTo(ViewModel, x => x.HeightMax)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderHeightMin.Value)
                .BindTo(ViewModel, x => x.HeightMin)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderWidthMin.Value)
                .BindTo(ViewModel, x => x.WidthMin)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderWidthMax.Value)
                .BindTo(ViewModel, x => x.WidthMax)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderLeftMin.Value)
                .BindTo(ViewModel, x => x.LeftMin)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderLeftMax.Value)
                .BindTo(ViewModel, x => x.LeftMax)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderTopMin.Value)
                .BindTo(ViewModel, x => x.TopMin)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderTopMax.Value)
                .BindTo(ViewModel, x => x.TopMax)
                .DisposeWith(d);
        });
    }
}