namespace Business.Handlers.Images.GetUploadStorageSignature.Dto
{
    public record GetUploadStorageSignatureRequest(
        string Folder,
        string FileName,
        string FileType
    );
}