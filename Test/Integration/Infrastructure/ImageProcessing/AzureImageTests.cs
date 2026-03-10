using Azure.AI.DocumentIntelligence;
using Infrastructure.Services.ImageProcessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Test.Configuration;
using Xunit.Abstractions;

namespace Test.Integration.Infrastructure.ImageProcessing
{
    public class AzureImageTests : BaseTest
    {
        private readonly AzureImageService _azureImageService;

        public AzureImageTests(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper) : base(factory,
            outputHelper)
        {
            var settings = factory.Services.GetRequiredService<IOptions<AzureImageSettings>>();
            var client = factory.Services.GetRequiredService<DocumentIntelligenceClient>();
            _azureImageService = new AzureImageService(settings, client);
        }

        [Theory]
        [InlineData("https://res.cloudinary.com/dfdq4xhtm/image/upload/v1769743643/IMG_7460_dq0ljj.jpg", false)]
        [InlineData("https://res.cloudinary.com/dfdq4xhtm/image/upload/v1769743643/IMG_7461_gdbooj.jpg", false)]
        public async Task ExtractImageAsync_ShouldReturnImageProcessResult(string url, bool isInvoice)
        {
            // Act
            var result = await _azureImageService.ExtractImageAsync(url, isInvoice);

            // Assert
            Assert.NotNull(result);
            
        }
    }
}