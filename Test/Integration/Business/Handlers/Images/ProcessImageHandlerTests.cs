using Test.Configuration;
using Test.Integration.Business.Handlers.BaseTests;
using Xunit.Abstractions;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Business.Handlers.Images.ProcessImage.Dto;

namespace Test.Integration.Business.Handlers.Images
{
    public class ProcessImageHandlerTests(
        CustomWebApplicationFactory factory,
        ITestOutputHelper outputHelper)
        : BaseTest(factory, outputHelper)
    {
        [Theory]
        [InlineData("https://res.cloudinary.com/dfdq4xhtm/image/upload/v1769743643/IMG_7460_dq0ljj.jpg", false)]
        [InlineData("https://res.cloudinary.com/dfdq4xhtm/image/upload/v1769743643/IMG_7461_gdbooj.jpg", false)]
        [InlineData("https://res.cloudinary.com/dfdq4xhtm/image/upload/v1769743643/receipt1_ft6ggx.jpg", false)]
        public async Task ProcessImage_ShouldReturnSuccess(string url, bool isInvoice)
        {
            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/images/process-image", new { url, isInvoice });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var processImageResponse =
                await response.Content.ReadFromJsonAsync<ProcessImageResponse>(JsonSerializerOptions);
            processImageResponse.Should().NotBeNull();

            var result = processImageResponse.Result;

            // Basic validations
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty("Receipt should have at least one item");
            result.Total.Value.Should().NotBeNull("Total should be extracted");
            result.Vendor.Name.Value.Should().NotBeNullOrWhiteSpace("Vendor name should be extracted");

            // Log the results for manual verification
            outputHelper.WriteLine($"URL: {url}");
            outputHelper.WriteLine($"Vendor: {result.Vendor.Name.Value}");
            outputHelper.WriteLine($"Total: {result.Total.Value}");
            outputHelper.WriteLine($"Items: {result.Items.Count}");
            foreach (var item in result.Items)
            {
                outputHelper.WriteLine($"  - {item.ItemName.Value}: {item.TotalPrice.Value}");
            }
        }
    }
}