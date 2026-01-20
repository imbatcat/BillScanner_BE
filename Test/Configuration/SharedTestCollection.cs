namespace Test.Configuration
{
    [CollectionDefinition("BillScannerTestCollection")]
    public class SharedTestCollection :
        IClassFixture<CustomWebApplicationFactory>
    {
    }
}