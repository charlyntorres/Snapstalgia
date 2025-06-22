using System;
using System.Collections.Generic;
using System.IO;

namespace Snap.Helpers
{
    public static class StickerPresets
    {
        private static readonly Dictionary<(string LayoutType, int StickerId), (string Behind, string Front)> StickerMap = new()
        {
            { ("1x2", 1), (Path.Combine("Assets", "Stickers", "1x2", "1x2_1_behind.png"),
                Path.Combine("Assets", "Stickers", "1x2", "1x2_1_front.png")) },
            { ("1x2", 2), (Path.Combine("Assets", "Stickers", "1x2", "1x2_2_behind.png"),
                Path.Combine("Assets", "Stickers", "1x2", "1x2_2_front.png")) },
            { ("1x2", 3), (Path.Combine("Assets", "Stickers", "1x2", "1x2_3_behind.png"),
                Path.Combine("Assets", "Stickers", "1x2", "1x2_3_front.png")) },
            { ("1x2", 4), (Path.Combine("Assets", "Stickers", "1x2", "1x2_4_behind.png"),
                Path.Combine("Assets", "Stickers", "1x2", "1x2_4_front.png")) },
            { ("1x2", 5), (Path.Combine("Assets", "Stickers", "1x2", "1x2_5_behind.png"),
                Path.Combine("Assets", "Stickers", "1x2", "1x2_5_front.png")) },

            { ("1x3", 1), (Path.Combine("Assets", "Stickers", "1x3", "1x3_1_behind.png"),
                Path.Combine("Assets", "Stickers", "1x3", "1x3_1_front.png")) },
            { ("1x3", 2), (Path.Combine("Assets", "Stickers", "1x3", "1x3_2_behind.png"),
                Path.Combine("Assets", "Stickers", "1x3", "1x3_2_front.png")) },
            { ("1x3", 3), (Path.Combine("Assets", "Stickers", "1x3", "1x3_3_behind.png"),
                Path.Combine("Assets", "Stickers", "1x3", "1x3_3_front.png")) },
            { ("1x3", 4), (Path.Combine("Assets", "Stickers", "1x3", "1x3_4_behind.png"),
                Path.Combine("Assets", "Stickers", "1x3", "1x3_4_front.png")) },
            { ("1x3", 5), (Path.Combine("Assets", "Stickers", "1x3", "1x3_5_behind.png"),
                Path.Combine("Assets", "Stickers", "1x3", "1x3_5_front.png")) },

            { ("1x4", 1), (Path.Combine("Assets", "Stickers", "1x4", "1x4_1_behind.png"),
                Path.Combine("Assets", "Stickers", "1x4", "1x4_1_front.png")) },
            { ("1x4", 2), (Path.Combine("Assets", "Stickers", "1x4", "1x4_2_behind.png"),
                Path.Combine("Assets", "Stickers", "1x4", "1x4_2_front.png")) },
            { ("1x4", 3), (Path.Combine("Assets", "Stickers", "1x4", "1x4_3_behind.png"),
                Path.Combine("Assets", "Stickers", "1x4", "1x4_3_front.png")) },
            { ("1x4", 4), (Path.Combine("Assets", "Stickers", "1x4", "1x4_4_behind.png"),
                Path.Combine("Assets", "Stickers", "1x4", "1x4_4_front.png")) },
            { ("1x4", 5), (Path.Combine("Assets", "Stickers", "1x4", "1x4_5_behind.png"),
                Path.Combine("Assets", "Stickers", "1x4", "1x4_5_front.png")) },
        };

        public static bool TryGetStickerPaths(string layoutType, int? stickerId, out string behindPath, out string frontPath)
        {
            behindPath = null;
            frontPath = null;

            if (string.IsNullOrWhiteSpace(layoutType) || !stickerId.HasValue)
                return false;

            if (!StickerMap.TryGetValue((layoutType, stickerId.Value), out var paths))
                return false;

            behindPath = paths.Behind;
            frontPath = paths.Front;
            return true;
        }
    }
}
