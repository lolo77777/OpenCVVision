﻿namespace OpenCVVision.Data
{
    public static class FilePath
    {
        public static class File
        {
            public const string LaserLineCaliYaml = @"Data\File\LaserLine\LightLaserCali.yaml";
            public const string ObjectNames = @"Data\File\Yolo\coco.names";
            public const string PatternCalibrateYaml = @"Data\File\GrayCode\patternCalibrate.yaml";
            public const string DetectorPrototxtPath = @"Data\File\WechatQRCodeModel\detect.prototxt";
            public const string DetectorCaffeModelPath = @"Data\File\WechatQRCodeModel\detect.caffemodel";
            public const string SuperResolutionPrototxtPath = @"Data\File\WechatQRCodeModel\sr.prototxt";
            public const string SuperResolutionCaffeModelPath = @"Data\File\WechatQRCodeModel\sr.caffemodel";
        }

        public static class Folder
        {
            public const string LaserLineBoardFolder = @"Data\Image\LaserLine\Board\";
            public const string LaserLineLightFolder = @"Data\Image\LaserLine\Light\";
            public const string LaserLineTestFolder = @"Data\Image\LaserLine\Test\";
            public const string YoloV3TestImage = @"Data\Image\YoloV3\";
            public const string PatternFolder = @"Data\Image\GrayCode\";
            public const string PhotometricStereoFolder = @"Data\Image\PhotometricStereo\";
            public const string PaddleXClsModelFodel = @"Data\File\PaddleX\cls\";
            public const string PaddleXDetModelFodel = @"Data\File\PaddleX\det\";
            public const string PaddleXClsImagelFodel = @"Data\Image\PaddleX\Cls\";
            public const string PaddleXDetImageFodel = @"Data\Image\PaddleX\Det\";
        }

        public static class Image
        {
            public const string LaserLineLightTemplate = @"Data\Image\LaserLine\Template.bmp";
            public const string Ta = @"Data\Image\ta.bmp";
            public const string QRCodeTest = @"Data\Image\WechatQRCode\QRCode.png";
        }
    }
}