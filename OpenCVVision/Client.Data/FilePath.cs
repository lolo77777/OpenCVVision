using System;

namespace Client.Data
{
    public static class FilePath
    {
        public static class File
        {
            public const string LaserLineCaliYaml = @"Data\File\LaserLine\LightLaserCali.yaml";
            public const string ObjectNames = @"Data\File\Yolo\coco.names";
            public const string PatternCalibrateYaml = @"Data\File\GrayCode\patternCalibrate.yaml";
        }

        public static class Folder
        {
            public const string LaserLineBoardFolder = @"Data\Image\LaserLine\Board\";
            public const string LaserLineLightFolder = @"Data\Image\LaserLine\Light\";
            public const string LaserLineTestFolder = @"Data\Image\LaserLine\Test\";
            public const string YoloV3TestImage = @"Data\Image\YoloV3\";
            public const string PatternFolder = @"Data\Image\GrayCode\";
        }

        public static class Image
        {
            public const string LaserLineLightTemplate = @"Data\Image\LaserLine\Template.bmp";
            public const string Ta = @"Data\Image\ta.bmp";
        }
    }
}