using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.Models.Gallery;
using Microsoft.EntityFrameworkCore;

namespace ImageGallery.Config
{
    public class Data
    {
        public List<string> Colors { get; set; }
        //public List<string> Tags { get; set; }
        //public List<string> DescriptionTags { get; set; }
        //public List<string> Categories { get; set; }
        //public List<ImageGalleryDb.Models.ImageGallery.Image> Images { get; set; }
        public List<string> TopTags { get; set; }
        public List<Category> SortedCategories { get; set; }
        public int ImagesCount { get; set; }
        public DbContextOptions Options { get; set; }
    }
}
