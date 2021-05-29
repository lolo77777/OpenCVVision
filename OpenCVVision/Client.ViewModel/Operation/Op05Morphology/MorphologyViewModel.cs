using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Client.Common;
using ReactiveUI.Fody.Helpers;
using System.Reflection.Metadata.Ecma335;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Client.ViewModel.Operation
{
    [OperationInfo("形态学1")]
    public class MorphologyViewModel : OperaViewModelBase
    {
        [Reactive] public string MorphShapeSelectValue { get; private set; }
        public ReadOnlyCollection<string> MorphShapesItems { get; set; }
        [Reactive] public string MorphTypeSelectValue { get; private set; }
        public ReadOnlyCollection<string> MorphTypesItems { get; private set; }

        [Reactive] public int SizeX { get; private set; }
        [Reactive] public int SizeY { get; private set; }

        public MorphologyViewModel()
        {
            MorphTypesItems = new ReadOnlyCollection<string>(Enum.GetNames(typeof(MorphTypes)));
            MorphShapesItems = new ReadOnlyCollection<string>(Enum.GetNames(typeof(MorphShapes)));

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(x => x.MorphTypeSelectValue, x => x.MorphShapeSelectValue, x => x.SizeX, x => x.SizeY)
                    .Throttle(TimeSpan.FromMilliseconds(150))
                    .Where(vt => CanOperat && MorphShapeSelectValue != null && MorphTypeSelectValue != null)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(vt => UpdataOutput(vt.Item3, vt.Item4, (MorphShapes)Enum.Parse(typeof(MorphShapes), vt.Item2), (MorphTypes)Enum.Parse(typeof(MorphTypes), vt.Item1)))
                    .Subscribe()
                    .DisposeWith(d);
                _imageDataManager.InputMatGuidSubject
                    .WhereNotNull()
                    .Where(guid => MorphShapeSelectValue != null && MorphTypeSelectValue != null)
                    .Do(guid => UpdataOutput(SizeX, SizeY, (MorphShapes)Enum.Parse(typeof(MorphShapes), MorphShapeSelectValue), (MorphTypes)Enum.Parse(typeof(MorphTypes), MorphTypeSelectValue)))
                    .Subscribe()
                    .DisposeWith(d);
            });
        }

        private void UpdataOutput(int sizex, int sizey, MorphShapes morphShapes, MorphTypes morphTypes)
        {
            SendTime(() =>
            {
                var element = _rt.T(Cv2.GetStructuringElement(morphShapes, new Size(sizex, sizey)));

                Mat reMat = _rt.NewMat();
                Cv2.MorphologyEx(_sigleSrc, reMat, morphTypes, element);

                _imageDataManager.OutputMatSubject.OnNext(reMat.Clone());
            });
        }
    }
}