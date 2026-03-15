using MediatR;

namespace Business.Handlers.Images.ProcessImage.Dto
{
    public record ProcessImageCommand :
        IRequest<ProcessImageResponse>
    {
        public Guid UserId { get; init; }
        public string Url { get; init; } = null!;
        public bool IsInvoice { get; init; }
    }
}