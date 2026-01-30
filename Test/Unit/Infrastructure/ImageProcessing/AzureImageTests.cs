using Infrastructure.Services.ImageProcessing;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.Unit.Infrastructure.ImageProcessing
{
    public class AzureImageTests
    {
        private readonly Mock<IOptions<AzureImageSettings>> optionsMock;

        public AzureImageTests()
        {
            optionsMock = new Mock<IOptions<AzureImageSettings>>();
            optionsMock.Setup(x => x.Value).Returns(new AzureImageSettings
            {
                Endpoint = "https://example.com",
                ApiKey1 = "example",
                ApiKey2 = "example",
                InvoiceModelId = "example",
                ReceiptModelId = "example",
            });
        }

        [Fact]
        public async Task ProcessImageAsync_ShouldReturnImageProcessResult()
        {
        }
    }
}