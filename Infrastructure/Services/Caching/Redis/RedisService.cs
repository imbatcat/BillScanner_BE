using System.Text.Json;
using System.Text.Json.Serialization;
using Business.Interfaces.Services;
using Infrastructure.MarkerInterfaces;
using StackExchange.Redis;

namespace Infrastructure.Services.Caching.Redis;

public class RedisService(IConnectionMultiplexer connectionMultiplexer) : ICachingService, ISingletonService
{
  private readonly JsonSerializerOptions jsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() }
  };

  public async Task<T?> GetAsync<T>(string key)
  {
    var db = connectionMultiplexer.GetDatabase();
    var value = await db.StringGetAsync(key);

    return value.IsNullOrEmpty
      ? default
      : JsonSerializer.Deserialize<T>(
        value.ToString(), jsonSerializerOptions);
  }

  public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration =null)
  {
    var db = connectionMultiplexer.GetDatabase();
    var jsonValue = JsonSerializer.Serialize(value, jsonSerializerOptions);
    await db.StringSetAsync(key, jsonValue, expiry: absoluteExpiration ?? Expiration.Default);
  }

  public async Task RemoveAsync(string key)
  {
    var db = connectionMultiplexer.GetDatabase();
    await db.KeyDeleteAsync(key);
  }

  public Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
  {
    var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
    var keys = server.Keys(pattern: pattern).Select(k => k.ToString());
    return Task.FromResult(keys);
  }
}