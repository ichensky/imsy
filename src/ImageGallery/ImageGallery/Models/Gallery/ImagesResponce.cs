using ImageGallery.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Models.Gallery
{
    public class ImagesResponce : ErrorResponce
    {
        public IEnumerable<Image> Images { get; set; }
    }
}
