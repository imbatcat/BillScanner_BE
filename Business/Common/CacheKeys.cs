using System.Security.Cryptography;
using System.Text;

namespace Business.Common;

public static class CacheKeys
{
    public static string GetProcessResultCacheKey(Guid userId, Guid resultId) => $"result:{userId}:{resultId}";
    public static Guid StableIdFromUrl(string url) => new Guid(MD5.HashData(Encoding.UTF8.GetBytes(url)));
}