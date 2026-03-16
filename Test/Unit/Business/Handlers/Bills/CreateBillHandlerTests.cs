using Business.Handlers.Bills.CreateBill;
using Business.Handlers.Bills.CreateBill.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Builders;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Test.Unit.Business.Handlers.Bills;

public class CreateBillHandlerTests
{
    private readonly Mock<IBuilderFactory> _builderFactoryMock;
    private readonly Mock<ICachingService> _cachingServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Bill>> _billRepositoryMock;
    private readonly Mock<IGenericRepository<BillExtractionResult>> _extractionResultRepositoryMock;
    private readonly CreateBillHandler _handler;

    public CreateBillHandlerTests()
    {
        _builderFactoryMock = new Mock<IBuilderFactory>();
        _cachingServiceMock = new Mock<ICachingService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _billRepositoryMock = new Mock<IGenericRepository<Bill>>();
        _extractionResultRepositoryMock = new Mock<IGenericRepository<BillExtractionResult>>();

        _unitOfWorkMock.Setup(u => u.Repository<Bill>()).Returns(_billRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<BillExtractionResult>()).Returns(_extractionResultRepositoryMock.Object);

        _handler = new CreateBillHandler(
            _builderFactoryMock.Object,
            _cachingServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateBillCommand { UserId = Guid.NewGuid(), ImgUrl = "test.jpg" };
        _cachingServiceMock.Setup(c => c.GetAsync<ImageProcessResult>(It.IsAny<string>()))
            .ReturnsAsync((ImageProcessResult)null!);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No result found for the given user and image URL.");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldPersistBillAndExtractionResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var imgUrl = "test.jpg";
        var command = new CreateBillCommand { UserId = userId, ImgUrl = imgUrl };
        
        var ocrResult = new ImageProcessResult
        {
            Vendor = new ExtractedVendor { Name = new ExtractedValue<string?> { Value = "Old Merchant" } },
            Total = new ExtractedValue<decimal?> { Value = 100m }
        };

        _cachingServiceMock.Setup(c => c.GetAsync<ImageProcessResult>(It.IsAny<string>()))
            .ReturnsAsync(ocrResult);

        var bill = new Bill { Id = Guid.NewGuid(), MerchantName = "Old Merchant" };
        var extractionResult = new BillExtractionResult { IsMerchantNameCorrect = true };

        var billBuilderMock = new Mock<IBillBuilder>();
        billBuilderMock.Setup(b => b.FromProcessResult(ocrResult)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.WithUserId(userId)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.WithImgUrl(imgUrl)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.WithUserEdits(command.UserEdits)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.Build()).Returns(bill);
        billBuilderMock.Setup(b => b.GetExtractionResult()).Returns(extractionResult);

        _builderFactoryMock.Setup(f => f.Builder<IBillBuilder>()).Returns(billBuilderMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.BillId.Should().Be(bill.Id);
        _billRepositoryMock.Verify(r => r.Insert(bill), Times.Once);
        _extractionResultRepositoryMock.Verify(r => r.Insert(It.Is<BillExtractionResult>(er => er.BillId == bill.Id)), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserEditsDiffer_ShouldFlagIncorrectExtraction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var imgUrl = "test.jpg";
        var command = new CreateBillCommand 
        { 
            UserId = userId, 
            ImgUrl = imgUrl,
            UserEdits = new UserEditsDto { MerchantName = "New Merchant" } 
        };
        
        var ocrResult = new ImageProcessResult
        {
            Vendor = new ExtractedVendor { Name = new ExtractedValue<string?> { Value = "Old Merchant" } }
        };

        _cachingServiceMock.Setup(c => c.GetAsync<ImageProcessResult>(It.IsAny<string>()))
            .ReturnsAsync(ocrResult);

        var bill = new Bill { Id = Guid.NewGuid(), MerchantName = "New Merchant" };
        var extractionResult = new BillExtractionResult { IsMerchantNameCorrect = false };

        var billBuilderMock = new Mock<IBillBuilder>();
        billBuilderMock.Setup(b => b.FromProcessResult(ocrResult)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.WithUserId(userId)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.WithImgUrl(imgUrl)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.WithUserEdits(command.UserEdits)).Returns(billBuilderMock.Object);
        billBuilderMock.Setup(b => b.Build()).Returns(bill);
        billBuilderMock.Setup(b => b.GetExtractionResult()).Returns(extractionResult);

        _builderFactoryMock.Setup(f => f.Builder<IBillBuilder>()).Returns(billBuilderMock.Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _extractionResultRepositoryMock.Verify(r => r.Insert(It.Is<BillExtractionResult>(er => er.IsMerchantNameCorrect == false)), Times.Once);
    }
}
