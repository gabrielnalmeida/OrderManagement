namespace OrderManagement.Application.OrderItems.Models;

public record SlimOrderItemDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}