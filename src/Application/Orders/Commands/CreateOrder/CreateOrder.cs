using FluentValidation.Results;
using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.OrderItems.Models;
using OrderManagement.Domain.Entities;
using ValidationException = OrderManagement.Application.Common.Exceptions.ValidationException;

namespace OrderManagement.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand : IRequest<int>
{
    public int BuyerId { get; init; }
    public IReadOnlyCollection<SlimOrderItemDto> Items { get; init; } = [];
}


public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationCache _cache;

    public CreateOrderCommandHandler(IApplicationDbContext context, IApplicationCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var buyerExists = await _context.Buyers.AnyAsync(x => x.Id == request.BuyerId, cancellationToken);

        if (!buyerExists)
        {
            throw new NotFoundException(nameof(Buyer), request.BuyerId.ToString());
        }

        var reqProductIds = request.Items
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .Where(x => reqProductIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (products.Count != reqProductIds.Count)
        {
            throw new ValidationException([
                new ValidationFailure("Items", "Um ou mais produtos não foram encontrados.")
            ]);
        }

        var items = request.Items
            .Select(x => new OrderItem(
                x.ProductId,
                x.Quantity,
                products[x.ProductId].Price
            ))
            .ToList();

        var order = new Order(request.BuyerId, items);

        _context.Orders.Add(order);

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, cancellationToken);


        return order.Id;
    }
}
