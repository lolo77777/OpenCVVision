using System;

namespace Client.Data
{
    public static class FilePath
    {
        public static class File
        {
            public const string LaserLineCaliYaml = @"Data\File\LaserLine\LightLaserCali.yaml";
            public const string ObjectNames = @"Data\File\YoloV3\coco.names";
            public const string YoloV3Cfg = @"Data\File\YoloV3\yolov3.cfg";
        }

        public static class Folder
        {
            public const string LaserLineBoardFolder = @"Data\Image\LaserLine\Board\";
            public const string LaserLineLightFolder = @"Data\Image\LaserLine\Light\";
            public const string LaserLineTestFolder = @"Data\Image\LaserLine\Test\";
            public const string YoloV3TestImage = @"Data\Image\YoloV3\";
        }

        public static class Image
        {
            public const string LaserLineLightTemplate = @"Data\Image\LaserLine\Template.bmp";
            public const string Ta = @"Data\Image\ta.bmp";
        }
    }
}