using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ImageGalleryDb.Models.Im
{
  
    public class ImageDescriptionCaption
    {
        [Key]
        public int Id { get; set; }
        public int DescriptionCaptionId { get; set; }
        public int ImageId { get; set; }
        public float Score { get; set; }

        public Image Image { get; set; }
        public DescriptionCaption DescriptionCaption { get; set; }
    }
}
