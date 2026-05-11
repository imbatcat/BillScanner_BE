namespace Business.Common;

public static class CacheKeys
{
    public static string GetBillCacheKey(Guid billId) => $"bill:{billId}";
    public static string GetProcessResultCacheKey(Guid userId, string url) => $"result:{userId}:{url}";
    public static string GetImageUrlCacheKey(string publicId) => $"image:{publicId}";
    public static string GetBillRefCacheKey(Guid userId, Guid billRefId) => $"bill-ref:{userId}:{billRefId}";
}