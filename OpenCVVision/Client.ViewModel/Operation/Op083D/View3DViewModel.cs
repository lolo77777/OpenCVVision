using Client.Model.Service.ImageProcess;

using HelixToolkit.SharpDX.Core;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using SharpDX;

using Splat;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Media3D;

using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace Client.ViewModel.Operation
{
    public class View3DViewModel : ViewModelBase, IRoutableViewModel
    {
        private GrayCodeProcess _grayCodeProcess;
        private Mat CameraMatrixMat;
        private Mat ProjecterMat;
        private Mat RMat1;
        private Mat TMat1;
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        private ResourcesTracker _rt;

        private static readonly AxisAngleRotation3D axisAngleRotation3D = new(new Vector3D(1, 0, 0), -90);
        [Reactive] public bool IsRun { get; set; }

        #region BindProperty

        public IEnumerable<int> SampleItems { get; } = new[] { 1, 2, 3, 4 };
        [Reactive] public int SampleSelectIndex { get; set; }
        [Reactive] public Camera CamDx { get; set; }
        [Reactive] public EffectsManager EffectsManager { get; set; }

        public Transform3D ModelTransform { get; set; } =
            new RotateTransform3D()
            {
                Rotation = axisAngleRotation3D
            };

        [Reactive] public PointGeometry3D PointGeometry { set; get; }
        public ReactiveCommand<Unit, Unit> DisplayCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NaviBackCommand { get; set; }
        public string UrlPathSegment { get; }
        public IScreen HostScreen { get; }

        #endregion BindProperty

        public View3DViewModel()
        {
            HostScreen = _resolver.GetService<IScreen>("MainHost");
        }

        protected override void SetupCommands()
        {
            base.SetupCommands();
            var displayCanExecute = this.WhenAnyValue(x => x.SampleSelectIndex, x => x.IsRun, (i, bol) => i >= 0 && !bol);
            DisplayCommand = ReactiveCommand.Create(Display, displayCanExecute);
            var mainScreen = _resolver.GetService<IScreen>("MainHost");
            NaviBackCommand = ReactiveCommand.CreateFromObservable(() => mainScreen.Router.Navigate.Execute(_resolver.GetService<ShellViewModel>()).Select(_ => Unit.Default));
        }

        protected override void SetupDeactivate()
        {
            base.SetupDeactivate();
            CamDx = null;
            _rt.Dispose();
            _grayCodeProcess = null;
            PointGeometry.ClearAllGeometryData();
            EffectsManager.DisposeAndClear();
            PointGeometry = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        protected override void SetupStart()
        {
            base.SetupStart();
            _rt = new();
            EffectsManager = new DefaultEffectsManager();
            PointGeometry = new PointGeometry3D();
            CamDx = new PerspectiveCamera()
            {
                LookDirection = new Vector3D(0, 0, -500),
                Position = new Point3D(0, 0, 500),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 3000
            };

            using var fr = new FileStorage(Data.FilePath.File.PatternCalibrateYaml, FileStorage.Modes.Read);
            CameraMatrixMat = _rt.T(fr["CameraMatrixMat"].ReadMat());
            ProjecterMat = _rt.T(fr["ProjecterMatrix"].ReadMat());

            RMat1 = _rt.T(fr["RMat"].ReadMat());
            TMat1 = _rt.T(fr["TMat"].ReadMat());
            _grayCodeProcess = new GrayCodeProcess(RMat1, TMat1, CameraMatrixMat, ProjecterMat);
        }

        #region PrivateFunction

        private void Display()
        {
            var path = Data.FilePath.Folder.PatternFolder + (SampleSelectIndex + 1);
            var mats = Directory.GetFiles(path).Skip(20).Take(20).Select(f => Cv2.ImRead(f, ImreadModes.Grayscale)).ToList();
            IsRun = true;
            Observable.Start(() =>
            {
                var pts = _grayCodeProcess.GetPointsAsync(mats);
                return updatePointAsync(pts.ToList());
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(vt =>
            {
                PointGeometry.Positions = vt.Vector3Collection;
                PointGeometry.Colors = vt.Color4Collection;
                PointGeometry.Indices = vt.IntCollection;
                IsRun = false;
            });
        }

        private (Vector3Collection Vector3Collection, Color4Collection Color4Collection, IntCollection IntCollection) updatePointAsync(List<Point3f> point3Fs)
        {
            var vector3s = new List<Vector3>();
            var color4s = new List<Color4>();
            var ints = new List<int>();
            var gap = 1;

            for (int i = 0; i < point3Fs.Count; i++)
            {
                var p = point3Fs[i];

                vector3s.Add(new Vector3(p.X, p.Z - 700, p.Y));
                color4s.Add(new Color4(new Vector3(120, 100, 20), 0.8f));
            }

            var vec3sCollec = new Vector3Collection(vector3s);
            var color4sCollec = new Color4Collection(color4s);
            var ids = new IntCollection(ints);

            return (vec3sCollec, color4sCollec, ids);
        }

        #endregion PrivateFunction
    }
}