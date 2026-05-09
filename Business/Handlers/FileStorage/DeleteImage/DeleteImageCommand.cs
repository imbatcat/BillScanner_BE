using MediatR;

namespace Business.Handlers.FileStorage.DeleteImage;

public record DeleteImageCommand(Guid UserId, string ImgUrl) : IRequest;
