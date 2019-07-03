using System;
using System.Collections.Generic;
using System.Text;

namespace ImageGalleryDb.Models.ImageGallery
{
   
    public class Image
    {
        public GoogleDrive GoogleDrive { get; set; }
        public string Title { get; set; }
        public MSVision.MSVision MSVision { get; set; }
        public string SeoId { get; set; }
    }
}
