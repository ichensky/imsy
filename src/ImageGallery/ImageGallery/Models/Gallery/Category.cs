using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Models.Gallery
{
    public class Category
    {
        public string Name { get; set; }
        public List<Category> Categories { get; set; }
        public string Id { get; set; }
    }
}
