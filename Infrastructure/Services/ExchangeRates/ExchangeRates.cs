using Newtonsoft.Json;

namespace Infrastructure.Services.ExchangeRates
{
    public class ExchangeRates
    {
        private readonly HttpClient _httpClient;
        private readonly string _urlString;

        public ExchangeRates(string exchangeRateApiKey, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _urlString = $"https://v6.exchangerate-api.com/v6/{exchangeRateApiKey}/latest/USD";
        }

        public async Task<ConversionRate?> ImportAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(_urlString);
                var result = JsonConvert.DeserializeObject<ApiObj>(json);
                return result?.Conversion_rates;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    // Resharper disable All

    public class ApiObj
    {
        public string? Result { get; set; }
        public string? Documentation { get; set; }
        public string? Terms_of_use { get; set; }
        public long? Time_last_update_unix { get; set; }
        public string? Time_last_update_utc { get; set; }
        public long? Time_next_update_unix { get; set; }
        public string? Time_next_update_utc { get; set; }
        public string? Base_code { get; set; }
        public ConversionRate? Conversion_rates { get; set; }
    }

    public class ConversionRate
    {
        public double AED { get; set; }
        public double ARS { get; set; }
        public double AUD { get; set; }
        public double BGN { get; set; }
        public double BRL { get; set; }
        public double BSD { get; set; }
        public double CAD { get; set; }
        public double CHF { get; set; }
        public double CLP { get; set; }
        public double CNY { get; set; }
        public double COP { get; set; }
        public double CZK { get; set; }
        public double DKK { get; set; }
        public double DOP { get; set; }
        public double EGP { get; set; }
        public double EUR { get; set; }
        public double FJD { get; set; }
        public double GBP { get; set; }
        public double GTQ { get; set; }
        public double HKD { get; set; }
        public double HRK { get; set; }
        public double HUF { get; set; }
        public double IDR { get; set; }
        public double ILS { get; set; }
        public double INR { get; set; }
        public double ISK { get; set; }
        public double JPY { get; set; }
        public double KRW { get; set; }
        public double KZT { get; set; }
        public double MXN { get; set; }
        public double MYR { get; set; }
        public double NOK { get; set; }
        public double NZD { get; set; }
        public double PAB { get; set; }
        public double PEN { get; set; }
        public double PHP { get; set; }
        public double PKR { get; set; }
        public double PLN { get; set; }
        public double PYG { get; set; }
        public double RON { get; set; }
        public double RUB { get; set; }
        public double SAR { get; set; }
        public double SEK { get; set; }
        public double SGD { get; set; }
        public double THB { get; set; }
        public double TRY { get; set; }
        public double TWD { get; set; }
        public double UAH { get; set; }
        public double USD { get; set; }
        public double UYU { get; set; }
        public double VND { get; set; }
        public double ZAR { get; set; }
    }
    // Resharper restore All
}
