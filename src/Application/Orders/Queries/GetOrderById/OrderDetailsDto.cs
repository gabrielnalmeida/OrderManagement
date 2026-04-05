using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Orders.Queries.GetOrderById;

public class OrderDetailsDto
{
    public int Id { get; init; }
    public int BuyerId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public DateTimeOffset Created { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyCollection<OrderItemDto> Items { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Order, OrderDetailsDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.GetSubTotal()));
        }
    }
}

public class OrderItemDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
}
