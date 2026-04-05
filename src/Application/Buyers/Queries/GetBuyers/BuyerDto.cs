using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Buyers.Queries.GetBuyers;

public class BuyerDto
{
    public BuyerDto() {}

    public int Id { get; init; }

    public string Name { get; init; } = default!;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Buyer, BuyerDto>();
        }
    }
}
