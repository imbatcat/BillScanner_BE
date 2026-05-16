using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;

namespace Business.Common;

public record CachedOcrResult(string ImageUrl, ImageProcessResult Data);
