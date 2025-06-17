namespace Snap.Helpers
{
    public static class LayoutPresets
    {
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

        public static (int Width, int Height) GetDimensions(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (400, 820), // Example dimensions for a 2x2 layout
                "2x3" => (400, 1100), // Example dimensions for a 3x3 layout
                "1x4" => (400, 1400) // Example dimensions for a 4x4 layout
,
                _ => throw new NotImplementedException()
            };
        }

        public static (int Rows, int Cols) GetGrid(string layoutType)
        {
            return layoutType switch
            {
                "1x2" => (1, 2), // 1 row, 2 columns
                "2x3" => (2, 3), // 2 rows, 3 columns
                "1x4" => (1, 4) // 1 row, 4 columns
,
                _ => throw new NotImplementedException()
            };
        }
    }
}
