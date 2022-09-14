namespace OpenCVVision.Model.Entity
{
    public class ImageData
    {
        public Guid ImageId { get; set; }
        public Mat ImageMat { get; set; }
        public string TxtMarker { get; set; }
    }
}