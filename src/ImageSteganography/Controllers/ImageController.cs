using ImageSteganography.Models;
using ImageSteganography.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageSteganography.Controllers;
[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    private readonly ILogger<ImageController> _logger;
    private readonly ImageSteganographyService _imageSteganographyService;

    public ImageController(
        ILogger<ImageController> logger,
        ImageSteganographyService imageSteganographyService)
    {
        _logger = logger;
        _imageSteganographyService = imageSteganographyService;
    }

    [HttpPost("EncodeImage")]
    public async Task<FileContentResult> EncodeImage([FromForm] EncodeImageRequest request)
    {
        var response = await _imageSteganographyService.EncodeImageAsync(request);

        FileContentResult file = new FileContentResult(/*response.EncodedImage*/ default, "application/octet-stream")
        {
            FileDownloadName = DateTime.Now.ToString() /*+ request.Image.FileName*/
        };

        return file;
    }

    [HttpPost("DecodeImage")]
    public async Task<string> DecodeImage([FromForm] DecodeImageRequest request)
    {
        var response = await _imageSteganographyService.DecodeImage(request);

        return response;
    }
}