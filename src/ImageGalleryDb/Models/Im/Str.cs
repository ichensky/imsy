using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ImageGalleryDb.Models.Im
{
    public class Str
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public float? TopScoreMin { get; set; }
        public float? TopScoreMinMin { get; set; }
        public List<ImageStr> ImageStrs { get; set; }

    }
}
