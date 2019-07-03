using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Models.Tags
{
    public class TagsResponce : Error.ErrorResponce
    {
        public List<string> Tags { get; set; }
    }
}
