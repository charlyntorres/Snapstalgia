using System;

namespace Snap.Models  
{
    public class CapturedPhoto
    {
        public string FileName { get; set; }        
        public DateTime CapturedAt { get; set; }    
        public string SessionId { get; set; }       
        public int Sequence { get; set; }           
        public string LayoutType { get; set; }      
        public int Width { get; set; }              
        public int Height { get; set; }             
    }
}
