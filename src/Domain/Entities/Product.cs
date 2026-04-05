namespace OrderManagement.Domain.Entities;

public class Product : BaseAuditableEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }

    private Product() { }

    public Product(string name, string? description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }

    public void Update(string name, string? description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }
}