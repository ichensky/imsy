using System.Collections.Generic;

namespace ImageGalleryDb.Models.ImageGallery.MSVision
{
    public class Description
    {
        public List<Caption> Captions { get; set; }
        public List<string> Tags { get; set; }
    }
}