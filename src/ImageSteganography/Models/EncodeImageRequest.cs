using ImageSteganography.Enums;

namespace ImageSteganography.Models;
public record EncodeImageRequest
{
    public long ChatId { get; set; }
    public string? Key { get; set; }
    public required IFormFile Image { get; set; }
    public ImageEncodeLevel EncodeLevel { get; set; }
    public required string Content { get; set; }
    public bool Unicode { get; set; }
}