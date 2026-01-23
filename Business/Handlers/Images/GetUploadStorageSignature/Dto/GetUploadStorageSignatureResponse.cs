namespace Business.Handlers.Images.GetUploadStorageSignature.Dto
{
    public record GetUploadStorageSignatureResponse(
        string Signature,
        long Timestamp,
        string ApiKey,
        string CloudName
    );
}