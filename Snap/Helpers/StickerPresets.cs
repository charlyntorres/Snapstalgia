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
                Path.Combine("Assets", "Stickers", "1x2", "1x2_1_front.png")) }

            // other stickers to be added
            // after adding stickers here, work on the user profile
            // then work on uploading
            // then work on deleting
            // then work on resolution
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
