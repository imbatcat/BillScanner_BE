using System.Net;
using System.Net.Http.Json;
using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using FluentAssertions;
using Test.Configuration;
using Test.Integration.Business.Handlers.BaseTests;
using Xunit.Abstractions;

namespace Test.Integration.Business.Handlers.Images
{
    [Collection("BillScannerTestCollection")]
    public class GetUploadStorageSignatureHandlerTest(
        CustomWebApplicationFactory factory,
        ITestOutputHelper outputHelper)
        : BaseTest(factory, outputHelper)
    {
        [Fact]
        public async Task GetUploadStorageSignature_ShouldReturnSuccess()
        {
            // Act
            var response = await Client.GetAsync("/api/v1/file-storage/signature");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result =
                await response.Content.ReadFromJsonAsync<GetUploadStorageSignatureResponse>(JsonSerializerOptions);

            result.Should().NotBeNull();
            result.Signature.Should().NotBeEmpty();
            result.Timestamp.Should().BeGreaterThan(0);
            result.ApiKey.Should().NotBeEmpty();
            result.CloudName.Should().NotBeEmpty();
        }
    }
}
