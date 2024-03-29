﻿using System.Windows.Forms;

namespace OpenCVVision.View.Operation;

/// <summary>
/// LoadFileView.xaml 的交互逻辑
/// </summary>
public partial class LoadFileView
{
    private readonly OpenFileDialog openFileDialog = new();

    public LoadFileView()
    {
        InitializeComponent();
        openFileDialog.Filter = "Image files (*.jpg;*.bmp;*.png)|*.jpg;*.bmp;*.png";
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.LoadImageCommand, v => v.btnLoadImage).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.TxtImageFilePath, v => v.FilePathTextBox.Text).DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.LoadFileConfirm,
                context => Observable.Return(openFileDialog.ShowDialog())
                .Do(result => context.SetOutput(result.Equals(DialogResult.OK) ? openFileDialog.FileName : string.Empty)

            )).DisposeWith(d);
        });
    }
}