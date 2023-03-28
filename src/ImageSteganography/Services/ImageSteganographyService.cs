using ImageSteganography.Models;
using ImageSteganography.Utilities;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImageSteganography.Services;
public class ImageSteganographyService
{
    private readonly string _startPoint = "@startpoint";
    private readonly string _endPoint = "@endpoint";

    public ImageSteganographyService()
    {

    }

    public async Task<EncodeImageResponse> EncodeImageAsync(EncodeImageRequest request)
    {
        string content = _startPoint + request.Content + _endPoint;

        using var memoryStream = new MemoryStream();
        await request.Image.CopyToAsync(memoryStream);

        var image = new Bitmap(memoryStream);
        var bmp = (Bitmap)image.Clone();

        //Textbit need to fill within parts cause its more faster
        string binaryContent = "";

        if (request.Unicode)
        {
            //Utf-32
            binaryContent += content.UnicodeStringToBinary();
        }
        else
        {
            binaryContent += content.ConvertStringToBinary();
        }

        //new number generates after fills two first bits
        byte[] newnumber = new byte[3];

        //proccess bits in image
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                if (binaryContent.Length.Equals(0))
                {
                    break;
                }

                Color pixel = image.GetPixel(x, y);
                var pixelColors = pixel.ToBinary();

                //note:some case 2 or 4 bits left we need to check if its lastone or not
                pixelColors.red = pixelColors.red.ReplaceFirstBits(!binaryContent.Length.Equals(0) ?
                    binaryContent.Substring(0, 2) : pixelColors.red.Substring(6, 2));
                binaryContent = binaryContent.Length.Equals(0) ? string.Empty : binaryContent.Remove(0, 2);

                pixelColors.green = pixelColors.green.ReplaceFirstBits(!binaryContent.Length.Equals(0) ?
                    binaryContent.Substring(0, 2) : pixelColors.green.Substring(6, 2));
                binaryContent = binaryContent.Length.Equals(0) ? string.Empty : binaryContent.Remove(0, 2);

                pixelColors.blue = pixelColors.blue.ReplaceFirstBits(!binaryContent.Length.Equals(0) ?
                    binaryContent.Substring(0, 2) : pixelColors.blue.Substring(6, 2));
                binaryContent = binaryContent.Length.Equals(0) ? string.Empty : binaryContent.Remove(0, 2);

                //Convert binary to number
                newnumber = (pixelColors.red + pixelColors.green + pixelColors.blue).ConvertBinaryStringToByteArray();
                var newColor = Color.FromArgb(int.Parse(newnumber[0].ToString()), int.Parse(newnumber[1].ToString()), int.Parse(newnumber[2].ToString()));
                bmp.SetPixel(x, y, newColor);
            }
        }

        var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);
        return new EncodeImageResponse()
        {
            EncodedImage = ms.ToArray(),
        };
    }

    public async Task<string> DecodeImage(DecodeImageRequest request)
    {
        using var memoryStream = new MemoryStream();
        await request.Image.CopyToAsync(memoryStream);
        Bitmap img = new Bitmap(memoryStream);

        //holds the new bits extract from image
        string bits = "";
        string extractedtext = "";
        bool shouldBreak = false;
        for (int x = 0; x < img.Width; x++)
        {
            if (shouldBreak)
            {
                break;
            }

            for (int y = 0; y < img.Height; y++)
            {

                if (extractedtext.Length >= _startPoint.Length && !extractedtext.Contains(_startPoint))
                {
                    //nothin in the picture
                    shouldBreak = true;
                    break;
                }

                if (extractedtext.Contains(_endPoint))
                {
                    extractedtext = extractedtext
                        .Replace(_startPoint, string.Empty)
                        .Replace(_endPoint, string.Empty);

                    shouldBreak = true;
                    break;
                }

                Color pixel = img.GetPixel(x, y);
                var colors = pixel.ToBinary();
                //read each pixel rgb first bits
                bits += colors.red.ReadFirstBits(2) + colors.green.ReadFirstBits(2) + colors.blue.ReadFirstBits(2);
                //if it isnt default
                if (request.Unicode)
                {
                    if (bits.Length >= 32)
                    {
                        extractedtext += bits.Substring(0, 32).GetUnicodeTextFromBinary();
                        bits = bits.Remove(0, 32);
                    }
                }
                //if its default
                else if ((bits.Length % 8).Equals(0))
                {
                    extractedtext += bits.BinaryToReadableText();
                    bits = null;
                }
            }
        }

        return extractedtext;
    }

}
