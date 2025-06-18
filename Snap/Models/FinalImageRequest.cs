namespace Snap.Models
{
    public class FinalImageRequest
    {
        public string SessionId { get; set; }
        public int Sequence { get; set; }        
        public int? StickerId { get; set; }
        public string? FrameColor { get; set; }
        public bool IncludeTimestamp { get; set; }
        public int? FilterId { get; internal set; }
        public string LayoutType { get; set; }
    }
}
