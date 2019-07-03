using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Models.Gallery
{
    public class CategoriesResponce : Error.ErrorResponce
    {
        public List<Category> Categories { get; set; }
    }
}
