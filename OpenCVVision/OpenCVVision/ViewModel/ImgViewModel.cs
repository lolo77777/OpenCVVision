using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Event;
using Stylet;

namespace OpenCVVision.ViewModel
{
    public class ImgViewModel : Screen, IHandle<DisImgEvent>
    {
        private IEventAggregator eventAggregator;
        private WriteableBitmap writeableBitmap;

        public ImgViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        #region BindProperty

        public WriteableBitmap ImgSource { get; set; }

        #endregion BindProperty

        #region Event

        public void Handle(DisImgEvent message)
        {
            var bitmap = message.Src?.ToBitmap();

            var pf = PixelFormats.Pbgra32;
            switch (bitmap.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.DontCare:
                    break;

                case System.Drawing.Imaging.PixelFormat.Max:
                    break;

                case System.Drawing.Imaging.PixelFormat.Indexed:
                    break;

                case System.Drawing.Imaging.PixelFormat.Gdi:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:

                    break;

                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    pf = PixelFormats.Bgr24;
                    break;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    pf = PixelFormats.Bgr32;
                    break;

                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    pf = PixelFormats.Gray8;
                    break;

                case System.Drawing.Imaging.PixelFormat.Alpha:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    break;

                case System.Drawing.Imaging.PixelFormat.PAlpha:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    pf = PixelFormats.Pbgra32;
                    break;

                case System.Drawing.Imaging.PixelFormat.Extended:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
                    break;

                case System.Drawing.Imaging.PixelFormat.Canonical:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    break;

                case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                    break;

                default:
                    break;
            }
            if (bitmap != null)
            {
                writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, pf, null);
                ImgHelp.WriteBitmap(ref writeableBitmap, ref bitmap);
                ImgSource = writeableBitmap;
            }
            BaseRes.CurSrc = message.Src.Clone();
        }

        #endregion Event
    }
}