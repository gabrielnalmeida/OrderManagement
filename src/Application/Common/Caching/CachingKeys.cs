namespace OrderManagement.Application.Common.Caching;

public static class CachingKeys
{
    public const string OrdersListPrefix = "orders:list:";
    public static string GetOrdersListCacheKey(int? buyerId, string? status, DateTime? createdFrom, DateTime? createdTo) =>
        $"{OrdersListPrefix}buyer:{buyerId?.ToString() ?? "null"}:status:{status ?? "null"}:from:{createdFrom?.ToUniversalTime().ToString("O") ?? "null"}:to:{createdTo?.ToUniversalTime().ToString("O") ?? "null"}";
}