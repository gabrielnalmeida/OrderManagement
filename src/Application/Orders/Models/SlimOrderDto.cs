using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Orders.Models;

public class SlimOrderDto
{
    public int Id { get; init; }
    public OrderStatus Status { get; init; }
    public DateTimeOffset Created { get; init; }
    public int TotalItems { get; init; }
    public decimal TotalAmount { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Order, SlimOrderDto>()
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity)))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)));
        }
    }
}
