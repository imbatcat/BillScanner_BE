namespace Business.Common;

public static class CacheKeys
{
    public static string GetBillCacheKey(Guid billId) => $"bill:{billId}";
    public static string GetProcessResultCacheKey(Guid userId, string url) => $"result:{userId}:{url}";
}