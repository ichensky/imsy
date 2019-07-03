using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ImageGalleryDb.Models.Im
{
    public class DescriptionCaption
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ImageDescriptionCaption> ImageDescriptionCaptions { get; set; }

    }
}
