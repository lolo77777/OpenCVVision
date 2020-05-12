using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCVVision.Model.Common;
using Stylet;

namespace OpenCVVision.ViewModel
{
    public class RecordEleViewModel : Screen
    {
        public string Name { get; set; }
        public WriteableBitmap ImgSource { get; set; }
        public WriteableBitmap writeableBitmap;

        public RecordEleViewModel(string name,Mat mat)
        {
            Name = name;
            update(mat);
        }

        private void update(Mat mat)
        {
            var bitmap = mat?.ToBitmap();
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
                writeableBitmap = new WriteableBitmap(bitmap.Width,bitmap.Height,96,96,pf,null);
                ImgHelp.WriteBitmap(ref writeableBitmap,ref bitmap);
                ImgSource = writeableBitmap;
            }
        }
    }
}