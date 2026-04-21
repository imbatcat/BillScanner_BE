using Business;
using Business.Handlers.Images.ProcessImage;
using Business.Handlers.Images.ProcessImage.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Services;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.Unit.Business.Handlers.Images;

public class ProcessImageHandlerTests
{
    private readonly Mock<IOptions<BusinessSettings>> settingsMock;
    private readonly Mock<ICachingService> cachingServiceMock;
    private readonly Mock<IImageExtractionService> imageProcessingServiceMock;
    private readonly ProcessImageHandler handler;
    private readonly BusinessSettings businessSettings;

    public ProcessImageHandlerTests()
    {
        settingsMock = new Mock<IOptions<BusinessSettings>>();
        cachingServiceMock = new Mock<ICachingService>();
        imageProcessingServiceMock = new Mock<IImageExtractionService>();
        businessSettings = new BusinessSettings { MinimumMissingFields = 2, CacheExpirationTimeInMinutes = 10 };

        settingsMock.Setup(x => x.Value).Returns(businessSettings);

        handler = new ProcessImageHandler(
            settingsMock.Object,
            cachingServiceMock.Object,
            imageProcessingServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_WhenExtractionIsSuccessfulAndValidationPasses()
    {
        // Arrange
        var command = new ProcessImageCommand
        {
            Url = "url",
            IsInvoice = true
        };

        var expectedResult = new ImageProcessResult
        {
            Vendor = new ExtractedVendor { Name = new ExtractedValue<string?> { Value = "Test Vendor" } },
            BillDate = new ExtractedValue<DateOnly?> { Value = DateOnly.FromDateTime(DateTime.Now) },
            Total = new ExtractedValue<decimal?> { Value = 100m },
            Items =
            [
                new ExtractedItem
                {
                    ItemName = new ExtractedValue<string?> { Value = "Item 1" },
                    TotalPrice = new ExtractedValue<decimal?> { Value = 100m }
                }
            ]
        };

        imageProcessingServiceMock
            .Setup(x => x.ExtractImageAsync(command.Url, command.IsInvoice))
            .ReturnsAsync(expectedResult);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        cachingServiceMock.Verify(x => x.SetAsync(command.Url, expectedResult, TimeSpan.FromMinutes(10)), Times.Once);
        response.Should().NotBeNull();
        response.Result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Handle_ShouldThrowScanRetryRequiredException_WhenTotalDoesNotMatchSumOfItems()
    {
        // Arrange
        var command = new ProcessImageCommand
        {
            Url = "url",
            IsInvoice = true
        };

        var expectedResult = new ImageProcessResult
        {
            Vendor = new ExtractedVendor { Name = new ExtractedValue<string?> { Value = "Test Vendor" } },
            BillDate = new ExtractedValue<DateOnly?> { Value = DateOnly.FromDateTime(DateTime.Now) },
            Total = new ExtractedValue<decimal?> { Value = 100m },
            Items =
            [
                new ExtractedItem
                {
                    ItemName = new ExtractedValue<string?> { Value = "Item 1" },
                    TotalPrice = new ExtractedValue<decimal?> { Value = 50m } // Sum (50) != Total (100)
                }
            ]
        };

        imageProcessingServiceMock
            .Setup(x => x.ExtractImageAsync(command.Url, command.IsInvoice))
            .ReturnsAsync(expectedResult);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        cachingServiceMock.Verify(x => x.SetAsync(command.Url, expectedResult, TimeSpan.FromMinutes(10)), Times.Never);
        await act.Should().ThrowAsync<ScanRetryRequiredException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowScanRetryRequiredException_WhenTooManyMissingFields()
    {
        // Arrange
        var localBusinessSettings = new BusinessSettings
        {
            MinimumMissingFields = 0,
            CacheExpirationTimeInMinutes = 10
        };
        settingsMock.Setup(x => x.Value).Returns(localBusinessSettings);
        var command = new ProcessImageCommand
        {
            Url = "url",
            IsInvoice = true
        };

        var expectedResult = new ImageProcessResult
        {
            Vendor = new ExtractedVendor { Name = new ExtractedValue<string?> { Value = null } }, // Missing
            BillDate = new ExtractedValue<DateOnly?> { Value = DateOnly.FromDateTime(DateTime.Now) },
            Total = new ExtractedValue<decimal?> { Value = 100m },
            Items =
            [
                new ExtractedItem
                {
                    ItemName = new ExtractedValue<string?> { Value = "Item 1" },
                    TotalPrice = new ExtractedValue<decimal?> { Value = 100m }
                }
            ]
        };

        imageProcessingServiceMock
            .Setup(x => x.ExtractImageAsync(command.Url, command.IsInvoice))
            .ReturnsAsync(expectedResult);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        cachingServiceMock.Verify(x => x.SetAsync(command.Url, expectedResult, TimeSpan.FromMinutes(10)), Times.Never);
        await act.Should().ThrowAsync<ScanRetryRequiredException>();
    }
}