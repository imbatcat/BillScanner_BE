using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using MediatR;

namespace Business.Handlers.Images.GetUploadStorageSignature
{
    public record GetUploadStorageSignatureRequest : IRequest<GetUploadStorageSignatureResponse>;
}