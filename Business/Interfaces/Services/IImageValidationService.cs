using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;

namespace Business.Interfaces.Services
{
    public interface IImageValidationService
    {
        public void ValidateImage(ImageProcessResult result);
    }
}