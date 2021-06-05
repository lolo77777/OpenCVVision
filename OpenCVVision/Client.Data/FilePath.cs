using System;

namespace Client.Data
{
    public static class FilePath
    {
        public static class Folder
        {
            public const string LaserLineBoardFolder = @"Data\Image\LaserLine\Board\";
            public const string LaserLineLightFolder = @"Data\Image\LaserLine\Light\";
        }

        public static class Image
        {
            public const string LaserLineLightTemplate = @"Data\Image\LaserLine\Template.bmp";
            public const string Ta = @"Data\Image\ta.bmp";
        }

        public static class Yaml
        {
            public const string LaserLineCaliYaml = @"Data\Yaml\LaserLine\LightLaserCali.yaml";
        }
    }
}