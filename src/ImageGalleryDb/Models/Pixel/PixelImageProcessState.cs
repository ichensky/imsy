namespace ImageGalleryDb.Models.Pixel
{
    public enum PixelImageProcessState
    {
        None=0,
        ImgFormatNotValid=1,
        ImageNotFound = 2,
        TooBigForGoogle=3
    }
}
