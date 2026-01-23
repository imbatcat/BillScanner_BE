using Business.Handlers.Images.GetUploadStorageSignature;
using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using Business.Interfaces.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Test.Unit.Business.Handlers.Images;

public class GetUploadStorageSignatureHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;

    private readonly QueryHandler _handler;

    public GetUploadStorageSignatureHandlerTests()
    {
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _handler = new QueryHandler(_fileStorageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSignature_WhenServiceReturnsSuccess()
    {
        // Arrange
        var request = new global::Business.Handlers.Images.GetUploadStorageSignature.GetUploadStorageSignatureRequest();
        var expectedResponse = new GetUploadStorageSignatureResponse(
            Signature: "test-signature",
            Timestamp: 123456789,
            ApiKey: "test-api-key",
            CloudName: "test-cloud-name"
        );

        _fileStorageServiceMock
            .Setup(x => x.GetUploadStorageSignatureAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        _fileStorageServiceMock.Verify(x => x.GetUploadStorageSignatureAsync(), Times.Once);
    }
}