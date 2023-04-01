namespace ImageSteganography.Models;

public record EncodeImageResponse
{
    public int TotalChars { get; init; }
    public int UsedChars { get; init; }
    public required Stream EncodedImage { get; init; }

    public EncodeImageResponse()
    {
    }
}
