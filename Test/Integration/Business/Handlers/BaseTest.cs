using System.Text.Json;
using System.Text.Json.Serialization;
using Test.Configuration;
using Xunit.Abstractions;

namespace Test.Integration.Business.Handlers
{
    [Collection("BillScannerTestCollection")]
    public abstract class BaseTest :
        IAsyncLifetime
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected readonly HttpClient Client;

        public BaseTest(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper)
        {
            Factory = factory;
            Factory.Output = outputHelper;
            Client = factory.HttpClient;
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        protected readonly JsonSerializerOptions JsonSerializerOptions;

        public async Task DisposeAsync()
        {
            Factory.Output = null; // Cleanup for the next test
            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await Factory.ResetDatabaseAsync();
        }
    }
}