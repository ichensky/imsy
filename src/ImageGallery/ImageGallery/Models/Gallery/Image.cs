using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Models.Gallery
{
    public class Image
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Tags { get; set; }
        public List<string> TopTags { get; set; }
        public List<string> DescriptionCaptions { get; set; }
        public List<string> DominantColors { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; internal set; }
        public string StrId { get; internal set; }
    }
}
