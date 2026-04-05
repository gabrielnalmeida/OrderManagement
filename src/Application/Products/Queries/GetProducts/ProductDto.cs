using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Products.Queries.GetProducts;

public class ProductDto
{
    public ProductDto() {}

    public int Id { get; init; }

    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public decimal Price { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductDto>();
        }
    }
}
