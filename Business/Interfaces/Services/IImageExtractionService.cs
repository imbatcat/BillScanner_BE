using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;

namespace Business.Interfaces.Services
{
    public interface IImageExtractionService
    {
        Task<ImageProcessResult> ExtractImageAsync(
            string url,
            bool isInvoice
        );
    }
}