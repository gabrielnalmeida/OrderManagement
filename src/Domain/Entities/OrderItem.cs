using System.ComponentModel.DataAnnotations;
using OrderManagement.Domain.Entities;

public class OrderItem : BaseAuditableEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() {}

    public OrderItem(int productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0) throw new ValidationException("A quantidade deve ser maior que zero.");
        if (unitPrice <= 0) throw new ValidationException("O preço unitário deve ser maior que zero.");

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public decimal GetSubTotal() => Quantity * UnitPrice;
}
