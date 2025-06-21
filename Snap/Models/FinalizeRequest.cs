namespace Snap.Models
{
    public class FinalizeRequest
    {
        public string FileName { get; set; }
        public int? FilterId { get; set; }
        public int? StickerId { get; set; }
        public string FrameColor { get; set; }
        public bool IncludeTimestamp { get; set; }
        public string LayoutType { get; set; }
    }

}
