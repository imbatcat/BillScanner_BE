using Business.Handlers.Images.GetUploadStorageSignature;
using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using Business.Interfaces.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Test.Unit.Business.Handlers.Images;

public class GetUploadStorageSignatureHandlerTests
{
    private readonly Mock<IFileStorageService> fileStorageServiceMock;

    private readonly QueryHandler handler;

    public GetUploadStorageSignatureHandlerTests()
    {
        fileStorageServiceMock = new Mock<IFileStorageService>();
        handler = new QueryHandler(fileStorageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSignature_WhenServiceReturnsSuccess()
    {
        // Arrange
        var request = new GetUploadStorageSignatureRequest(true);
        var expectedResponse = new GetUploadStorageSignatureResponse(
            Signature: "test-signature",
            Timestamp: 123456789,
            ApiKey: "test-api-key",
            CloudName: "test-cloud-name"
        );

        fileStorageServiceMock
            .Setup(x => x.GetUploadStorageSignatureAsync(request.IsInvoice))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        fileStorageServiceMock.Verify(x => x.GetUploadStorageSignatureAsync(request.IsInvoice), Times.Once);
    }
}