using Infrastructure.MarkerInterfaces;

namespace Infrastructure.Services.Caching.Redis;

public class RedisSettings : IAppSettings
{
  public string ConnectionString { get; init; }
}