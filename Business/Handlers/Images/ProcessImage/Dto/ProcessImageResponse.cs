using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;

namespace Business.Handlers.Images.ProcessImage.Dto
{
    public record ProcessImageResponse(
        string Url,
        ImageProcessResult Result);
}