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
                "1x2" => (400, 740),
                "1x3" => (400, 1035),
                "1x4" => (400, 1330),
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
                "1x2" => (360, 280),
                "1x3" => (360, 280),
                "1x4" => (360, 280),
                _ => (320, 240)
            };
        }
    }
}
