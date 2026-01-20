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
        }

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