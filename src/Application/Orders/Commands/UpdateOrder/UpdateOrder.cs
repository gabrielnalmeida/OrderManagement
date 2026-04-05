using FluentValidation.Results;
using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.OrderItems.Models;
using ValidationException = OrderManagement.Application.Common.Exceptions.ValidationException;

namespace OrderManagement.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand : IRequest
{
    public int Id { get; init; }
    public IReadOnlyCollection<SlimOrderItemDto> Items { get; init; } = [];
}

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationCache _cache;

    public UpdateOrderCommandHandler(IApplicationDbContext context, IApplicationCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, order);

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
                products[x.ProductId].Price))
            .ToList();

        order.ReplaceItems(items);

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, cancellationToken);
    }
}
