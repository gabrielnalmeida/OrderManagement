using OrderManagement.Application.Orders.Models;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Buyers.Queries.GetBuyers;

public class BuyerDto
{
    public BuyerDto() {}

    public int Id { get; init; }

    public string Name { get; init; } = default!;

    public List<SlimOrderDto> Orders { get; private set; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Buyer, BuyerDto>()
                .ForMember(dest => dest.Orders, opt => opt.MapFrom(src => src.Orders));
        }
    }
}
