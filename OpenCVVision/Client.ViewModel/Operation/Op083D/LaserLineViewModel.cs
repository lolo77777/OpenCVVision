using Client.Data;
using Client.Model.Service.ImageProcess;

using OpenCvSharp;

using ReactiveUI;

using Splat;

using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Client.ViewModel.Operation
{
    [OperationInfo(8.1, "线激光中心", MaterialDesignThemes.Wpf.PackIconKind.LaserPointer)]
    public class LaserLineViewModel : OperaViewModelBase
    {
        private LightPlaneCal _lightPlaneCal;
        private LightPlaneCalibrate _lightPlaneCalibrate;
        public ReactiveCommand<Unit, Unit> CalibrateTestCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LaserLigthCalCommand { get; set; }

        protected override void SetupStart()
        {
            base.SetupStart();
            _lightPlaneCalibrate = _resolver.GetService<LightPlaneCalibrate>();
            _lightPlaneCal = _resolver.GetService<LightPlaneCal>();
        }

        protected override void SetupCommands()
        {
            base.SetupCommands();
            CalibrateTestCommand = ReactiveCommand.Create(CalibrateTest);
            LaserLigthCalCommand = ReactiveCommand.Create(CalTest);
        }
        protected override void SetupDeactivate()
        {
            base.SetupDeactivate();
            _lightPlaneCal = null;
            _lightPlaneCalibrate = null;
        }
        #region PrivateFunction

        private void CalibrateTest()
        {
            _lightPlaneCalibrate.Calibrate();
        }

        private void CalTest()
        {
            SendTime(() =>
            {
                using var fr = new FileStorage(FilePath.File.LaserLineCaliYaml, FileStorage.Modes.Read);
                var _cameraMatrixMat = fr["CameraMatrixMat"].ReadMat();

                var _lightPlaneCoeffient = fr["LightPlaneCoeffient"].ReadMat();
                var _cameraToLightPlaneMat = fr["CameraToLightPlaneMat"].ReadMat();
                var _templete = Cv2.ImRead(FilePath.Image.LaserLineLightTemplate, ImreadModes.Grayscale).PyrDown().PyrDown().PyrDown().PyrDown().GaussianBlur(new Size(3, 3), 0.24);
                var files = Directory.GetFiles(FilePath.Folder.LaserLineTestFolder).Select(str => Cv2.ImRead(str, ImreadModes.Grayscale));
                var num = 0;
                Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000))
                    .Take(files.Count())
                    .Select(i => files.ElementAt(num))
                    .Do(mat =>
                    {
                        num++;
                        var re = _lightPlaneCal.GetResultGray(mat, _templete, _cameraToLightPlaneMat, _cameraMatrixMat, _lightPlaneCoeffient);
                        _imageDataManager.OutputMatSubject.OnNext(re.thinMat);
                    })
                    .Subscribe();
            });
        }

        #endregion PrivateFunction
    }
}