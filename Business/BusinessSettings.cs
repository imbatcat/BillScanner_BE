namespace Business
{
    public class BusinessSettings
    {
        public int MinimumMissingFields { get; init; } = 2;
        public int CacheExpirationTimeInMinutes { get; init; } = 10;
    }
}