using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

namespace Client.Model.Entity
{
    public class ImageData
    {
        public Guid ImageId { get; set; }
        public Mat ImageMat { get; set; }
        public string TxtMarker { get; set; }
    }
}