using Business.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.ExchangeRates;

public class ExchangeRateService(
    IOptions<ExchangeRatesSettings> settings,
    HttpClient httpClient) : IExchangeRateService
{
    private ConversionRate? _cachedRates;

    private async Task<ConversionRate> GetRatesAsync()
    {
        if (_cachedRates is not null) return _cachedRates;
        var helper = new ExchangeRates(settings.Value.ApiKey, httpClient);
        _cachedRates = await helper.ImportAsync()
            ?? throw new InvalidOperationException("Failed to fetch exchange rates.");
        return _cachedRates;
    }

    public async Task<decimal> ConvertToVndAsync(decimal amount, string fromCurrency)
    {
        if (fromCurrency.Equals("VND", StringComparison.OrdinalIgnoreCase)) return amount;

        var rates = await GetRatesAsync();

        var prop = typeof(ConversionRate).GetProperty(fromCurrency.ToUpper())
            ?? throw new NotSupportedException($"Currency '{fromCurrency}' is not supported.");

        var fromRate = (double)prop.GetValue(rates)!;
        return amount / (decimal)fromRate * (decimal)rates.VND;
    }
}
