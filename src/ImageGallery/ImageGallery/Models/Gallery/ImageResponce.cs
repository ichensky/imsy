using ImageGallery.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Models.Gallery
{
    public class ImageResponce : ErrorResponce
    {
        public Image Image { get; set; }
    }
}
