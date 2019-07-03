using System.Collections.Generic;

namespace ImageGalleryDb.Models.ImageGallery.MSVision
{
    public class MSVision
    {
        public List<Tag> Tags { get; set; }
        public Color Color { get; set; }
        public Description Description { get; set; }
        public List<Category> Categories { get; set; }
    }
}