using System;
using System.Collections.Generic;

namespace Snap.Helpers
{
    public static class LayoutPresets
    {
        // Supported layouts: row x column
        private static readonly HashSet<string> SupportedLayouts = new()
        {
            "1x2",
            "2x3",
            "1x4"
        };

        public static bool IsValidLayout(string layoutType)
        {
            return !string.IsNullOrEmpty(layoutType) && SupportedLayouts.Contains(layoutType);
        }

        // Optional: returns (width, height) in pixels, can be adjusted per layout if needed
        public static (int Width, int Height) GetDimensions(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (400, 820),
                "2x3" => (800, 1100),
                "1x4" => (400, 1400),
                _ => throw new NotImplementedException($"Dimensions not set for layout {layoutType}")
            };
        }

        // Returns number of rows and columns for given layout string
        public static (int Rows, int Cols) GetGrid(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (1, 2),
                "2x3" => (2, 3),
                "1x4" => (1, 4),
                _ => throw new NotImplementedException($"Grid not set for layout {layoutType}")
            };
        }
    }
}
