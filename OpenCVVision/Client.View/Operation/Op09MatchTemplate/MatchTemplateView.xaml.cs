﻿using Client.ViewModel.Operation;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.View.Operation
{
    /// <summary>
    /// MatchTemplateView.xaml 的交互逻辑
    /// </summary>
    public partial class MatchTemplateView : ReactiveUserControl<MatchTemplateViewModel>
    {
        public MatchTemplateView()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.MatchMethodItems, v => v.cbxMatchMethod.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ImageItems, v => v.cbxImageItems1.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ImageItems, v => v.cbxImageItems2.ItemsSource).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.MatchCommand, v => v.btnImage).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AngleStep, v => v.txtMatchThre.Text).DisposeWith(d);
                this.WhenAnyValue(x => x.cbxMatchMethod.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, vm => vm.MatchMethodSelectValue)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbxImageItems1.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, vm => vm.SrcImageMarkTxt)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbxImageItems2.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, vm => vm.TemplateImageMarkTxt)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbIsEnableAngle.IsChecked)
                    .BindTo(ViewModel, vm => vm.IsEnableAngle)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderThresh.Value)
                    .Select(v => v / 100)
                    .BindTo(ViewModel, vm => vm.MatchThreshold)
                    .DisposeWith(d);
            });
        }
    }
}