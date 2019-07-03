using System.Collections.Generic;

namespace ImageGalleryDb.Models.ImageGallery.MSVision
{
    public enum DominantColorType
    {
        None = 0,
        Background = 1,
        Foreground = 2,
        BackgroundAndForeground=3
    }
    public class DominantColor
    {
        public string Name { get; set; }
        public DominantColorType DominantColorType { get; set; }
    }
    public class Color
    {
        public List<DominantColor> DominantColors { get; set; }
        public bool IsBWImg { get; set; }
        public string AccentColor { get; set; }
    }
}