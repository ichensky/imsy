using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ImageGalleryDb.Models.Im
{
  
    public class ImageCategory
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int ImageId { get; set; }
        public float Score { get; set; }

        public Image Image { get; set; }
        public Category Category { get; set; }
    }
}
