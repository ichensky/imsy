using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Models.Gallery
{
    public class ImageQuery
    {
        [JsonProperty("query")]
        public string Query { get; set; }
        [JsonProperty("page")]
        public int Page { get; set; }
    }
}
