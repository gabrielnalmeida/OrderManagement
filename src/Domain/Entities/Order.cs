using System.ComponentModel.DataAnnotations;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public int BuyerId { get; private set; }
    public Buyer Buyer { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.Started;

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order() {}

    public Order(int buyerId, IEnumerable<OrderItem> items)
    {
        BuyerId = buyerId;
        ReplaceItems(items);
        Status = OrderStatus.Started;
    }

    public void ReplaceItems(IEnumerable<OrderItem> items)
    {
        EnsureCanBeChanged();

        var normalizedItems = items.ToList();

        if (!normalizedItems.Any())
        {
            throw new ValidationException("Um pedido deve conter pelo menos um item.");
        }

        _items.Clear();
        _items.AddRange(items);
    }

    public void Cancel()
    {
        if (Status is not OrderStatus.Started and not OrderStatus.Processed)
        {
            throw new ValidationException("Apenas pedidos iniciados ou processados podem ser cancelados.");
        }

        Status = OrderStatus.Cancelled;
    }

    public void Process()
    {
        if (Status != OrderStatus.Started)
        {
            throw new ValidationException("Apenas pedidos iniciados podem ser processados.");
        }

        Status = OrderStatus.Processed;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processed)
        {
            throw new ValidationException("Apenas pedidos processados podem ser enviados.");
        }

        Status = OrderStatus.Shipped;
    }

    public decimal GetTotal()
    {
        return _items.Sum(i => i.Quantity * i.UnitPrice);
    }

    public bool CanBeChanged() => Status == OrderStatus.Started;

    private void EnsureCanBeChanged()
    {
        if (!CanBeChanged())
        {
            throw new ValidationException("Apenas pedidos não processados podem ser alterados.");
        }
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}