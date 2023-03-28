namespace ImageSteganography.Models;
public record DecodeImageRequest
{
    public required IFormFile Image { get; set; }
    public bool Unicode { get; set; }
}