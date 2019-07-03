using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ImageGalleryDb.Models.Im
{
    public class ImageStr
    {
        [Key]
        public int Id { get; set; }
        public int StrId { get; set; }
        public int ImageId { get; set; }
        public float? Score { get; set; }
        public Boolean IsTag { get; set; }
        public Boolean IsDescriptionTag { get; set; }
        public Boolean IsDominantColor { get; set; }
        public Boolean IsDominantColorBackground { get; set; }
        public Boolean IsDominantColorForeground { get; set; }

        public Image Image { get; set; }
        public Str Str { get; set; }
    }
}
