using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Orders.Queries.GetOrders;

public class OrderSummaryDto
{
    public int Id { get; init; }
    public int BuyerId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public DateTimeOffset Created { get; init; }
    public int TotalItems { get; init; }
    public decimal TotalAmount { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.BuyerName, opt => opt.MapFrom(src => src.Buyer.Name))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity)))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)));
        }
    }
}
