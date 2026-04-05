using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand(int Id) : IRequest;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationCache _cache;
    
    public CancelOrderCommandHandler(IApplicationDbContext context, IApplicationCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, order);

        order.Cancel();

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, cancellationToken);
    }
}
