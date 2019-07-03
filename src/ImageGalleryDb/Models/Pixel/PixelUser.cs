using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ImageGalleryDb.Models.Pixel
{
    public class PixelUser
    {
        [Key]
        public Guid PixelUserId { get; set; }
        public string NickName { get; set; }
        public string UserName { get; set; }
        public List<PixelImage> PixelImages { get; set; }

    }
}
