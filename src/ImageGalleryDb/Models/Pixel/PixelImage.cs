using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ImageGalleryDb.Models.Pixel
{
    public class PixelImage
    {
        [Key]
        public Guid PixelImageId { get; set; }
        public Guid PixelUserId { get; set; }
        public string Url { get; set; }
        public string MetaUrl { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string GoogleId { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string GrayscalePerceptualHash { get; set; }
        public string MSVision { get; set; }
        public PixelImageProcessState? ProcessState { get; set; }

        public PixelUser PixelUser { get; set; }
    }
}
