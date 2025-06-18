using System;

namespace Snap.Models 
{
    public class PhotoUploadRequest
    {        
        public string SessionId { get; set; }     
        public int Sequence { get; set; }         
        public string LayoutType { get; set; }
        public IFormFile File { get; set; }
    }
}
