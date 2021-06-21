using Client.Model.Service.ImageProcess;

using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using SharpDX;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

using UnmanageUtility;

using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace Client.ViewModel.Operation
{
    public class View3DViewModel : ReactiveObject
    {
        private GrayCodeProcess _grayCodeProcess;

        #region BindProperty

        public IEnumerable<int> SampleItems { get; } = new[] { 1, 2, 3, 4 };
        [Reactive] public int SampleSelectIndex { get; set; }
        [Reactive] public Camera CamDx { get; set; }
        [Reactive] public EffectsManager EffectsManager { get; set; } = new DefaultEffectsManager();

        public Transform3D ModelTransform { get; set; } =
            new RotateTransform3D()
            {
                Rotation = new System.Windows.Media.Media3D.AxisAngleRotation3D(
                new Vector3D(1, 0, 0), -90)
            };

        [Reactive] public PointGeometry3D PointGeometry { set; get; } = new PointGeometry3D();
        public ReactiveCommand<Unit, Unit> DisplayCommand { get; set; }

        #endregion BindProperty

        public View3DViewModel()
        {
            SetupStart();
            SetupCommand();
        }

        private void SetupCommand()
        {
            var displayCanExecute = this.WhenAnyValue(x => x.SampleSelectIndex, i => i >= 0);
            DisplayCommand = ReactiveCommand.Create(Display, displayCanExecute);
        }

        private void SetupStart()
        {
            CamDx = new PerspectiveCamera()
            {
                LookDirection = new Vector3D(0, 0, -500),
                Position = new Point3D(0, 0, 500),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 3000
            };
            var datapath = Data.FilePath.File.PatternCalibrateYaml;
            using var fr = new FileStorage(datapath, FileStorage.Modes.Read);
            var CameraMatrixMat = fr["CameraMatrixMat"].ReadMat();
            var ProjecterMat = fr["ProjecterMatrix"].ReadMat();

            var RMat1 = fr["RMat"].ReadMat();
            var TMat1 = fr["TMat"].ReadMat();
            _grayCodeProcess = new GrayCodeProcess(RMat1, TMat1, CameraMatrixMat, ProjecterMat);
        }

        public void Display()
        {
            var path = Data.FilePath.Folder.PatternFolder + (SampleSelectIndex + 1);
            var mats = Directory.GetFiles(path).Skip(20).Take(20).Select(f => Cv2.ImRead(f, ImreadModes.Grayscale)).ToList();

            var pts = _grayCodeProcess.GetPointsAsync(mats);
            var (v3s, c4s, ids) = updatePointAsync(pts.ToList());

            PointGeometry.Positions = v3s;
            PointGeometry.Colors = c4s;
            PointGeometry.Indices = ids;
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
    }
}