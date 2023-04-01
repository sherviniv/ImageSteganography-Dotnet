namespace ImageSteganography.Models;
public class DecodeImageRequest
{
    public Stream Image { get; set; }
    public bool Unicode { get; set; }
}