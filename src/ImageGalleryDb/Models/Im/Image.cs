using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ImageGalleryDb.Models.Im
{
    public enum Boolean {
        False=0,
        True=1
    }
    public class Image
    {
        [Key]
        public int Id { get; set; }
        public string GoogleDriveId { get; set; }
        public string Title { get; set; }
        public string SeoId { get; set; }
        public int GoogleDriveWidth { get; set; }
        public int GoogleDriveHeight { get; set; }
        public Boolean MSVisionIsBWImg { get; set; }
        public string MSVisionAccentColor { get; set; }
        public List<ImageStr> ImageStrs { get; set; }
        public List<ImageCategory> ImageCategories { get; set; }
        public List<ImageDescriptionCaption> ImageDescriptionCaptions { get; set; }
    }
}
