using System.Text.Json;
using System.Text.Json.Serialization;
using Test.Configuration;
using Xunit.Abstractions;

namespace Test.Integration.Business.Handlers.BaseTests
{
    [Collection("BillScannerTestCollection")]
    public abstract class BaseTest :
        IAsyncLifetime
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected readonly HttpClient Client;

        protected readonly JsonSerializerOptions JsonSerializerOptions;

        protected BaseTest(CustomWebApplicationFactory factory, ITestOutputHelper outputHelper)
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


        public virtual async Task DisposeAsync()
        {
            Factory.Output = null; // Cleanup for the next test
            await Task.CompletedTask;
        }

        public virtual async Task InitializeAsync()
        {
            await Factory.ResetDatabaseAsync();
        }
    }
}