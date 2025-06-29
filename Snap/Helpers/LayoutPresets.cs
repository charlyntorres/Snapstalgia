using System;
using System.Collections.Generic;

namespace Snap.Helpers
{
    public static class LayoutPresets
    {
        private static readonly Dictionary<int, string> LayoutMap = new()
        {
            { 2, "1x2" },
            { 3, "1x3" },
            { 4, "1x4" }
        };

        public static bool IsValidLayout(int layoutType)
        {
            return LayoutMap.ContainsKey(layoutType);
        }

        public static (int Width, int Height) GetDimensions(int layoutType)
        {
            return layoutType switch
            {
                2 => (250, 373),
                3 => (250, 566),
                4 => (250, 759),
                _ => throw new NotImplementedException($"Dimensions not set for layout type {layoutType}")
            };
        }

        public static (int rows, int cols) GetGrid(int layoutType)
        {
            return layoutType switch
            {
                2 => (2, 1),
                3 => (3, 1),
                4 => (4, 1),
                _ => throw new ArgumentException("Invalid layout type.")
            };
        }

        public static (int Width, int Height) GetPhotoSize(int layoutType)
        {
            return layoutType switch
            {
                2 => (250, 180),
                3 => (250, 180),
                4 => (250, 180),
                _ => (250, 180)
            };
        }

        public static (int FinalWidth, int FinalHeight) GetFinalImageSize(int layoutType)
        {
            return layoutType switch
            {
                2 => (275, 444),
                3 => (275, 637),
                4 => (275, 830),
                _ => throw new NotImplementedException($"Final image size not set for layout type {layoutType}")
            };
        }

        public static string GetLayoutCode(int layoutType)
        {
            return LayoutMap.TryGetValue(layoutType, out var code) ? code : "1x4";
        }
    }
}
