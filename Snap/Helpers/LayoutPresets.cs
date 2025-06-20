using System;
using System.Collections.Generic;

namespace Snap.Helpers
{
    public static class LayoutPresets
    {
        private static readonly HashSet<string> SupportedLayouts = new()
        {
            "1x2",
            "1x3",
            "1x4"
        };

        public static bool IsValidLayout(string layoutType)
        {
            return !string.IsNullOrEmpty(layoutType) && SupportedLayouts.Contains(layoutType);
        }
        
        public static (int Width, int Height) GetDimensions(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (250, 373),
                "1x3" => (250, 566),
                "1x4" => (250, 759),
                _ => throw new NotImplementedException($"Dimensions not set for layout {layoutType}")
            };
        }

        public static (int Rows, int Cols) GetGrid(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (2, 1),
                "1x3" => (3, 1),
                "1x4" => (4, 1),
                _ => throw new NotImplementedException($"Grid not set for layout {layoutType}")
            };
        }

        public static (int Width, int Height) GetPhotoSize(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (250, 180),
                "1x3" => (250, 180),
                "1x4" => (250, 180),
                _ => (250, 180)
            };
        }

        public static (int FinalWidth, int FinalHeight) GetFinalImageSize(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (275, 444),
                "1x3" => (275, 637),
                "1x4" => (275, 830),
                _ => throw new NotImplementedException($"Final image size not set for layout {layoutType}")
            };
        }
    }
}
