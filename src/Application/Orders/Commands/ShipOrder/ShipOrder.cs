using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Orders.Commands.ShipOrder;

public record ShipOrderCommand(int Id) : IRequest;

public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationCache _cache;

    public ShipOrderCommandHandler(IApplicationDbContext context, IApplicationCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, order);

        order.Ship();

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, cancellationToken);
    }
}
