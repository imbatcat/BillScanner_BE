namespace Business.Interfaces.Services;

public interface ICachingService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null);

    Task RemoveAsync(string key);

    Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);
}