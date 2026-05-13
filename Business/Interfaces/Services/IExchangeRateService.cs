namespace Business.Interfaces.Services;

public interface IExchangeRateService
{
    Task<decimal> ConvertToVndAsync(decimal amount, string fromCurrency);
}
