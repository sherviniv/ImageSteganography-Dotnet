using ImageSteganography.Enums;

namespace ImageSteganography.Models;
public class EncodeImageRequest
{
    public long ChatId { get; set; }
    public string? Key { get; set; }
    public Stream Image { get; set; }
    public ImageEncodeLevel EncodeLevel { get; set; }
    public string Content { get; set; }
    public bool Unicode { get; set; }
}