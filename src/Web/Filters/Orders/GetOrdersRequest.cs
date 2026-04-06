using OrderManagement.Domain.Enums;

namespace OrderManagement.Web.Filters.Orders;

public record GetOrdersRequest
{
    public int? BuyerId { get; init; }
    public OrderStatus? Status { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}