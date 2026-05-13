using Infrastructure.MarkerInterfaces;

namespace Infrastructure.Services.ExchangeRates
{
    public class ExchangeRatesSettings : IAppSettings
    {
        public string ApiKey { get; set; } = null!;
    }
}
