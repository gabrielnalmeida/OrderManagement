using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Orders.Commands.ProcessOrder;

public record ProcessOrderCommand(int Id) : IRequest;

public class ProcessOrderCommandHandler : IRequestHandler<ProcessOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationCache _cache;

    public ProcessOrderCommandHandler(IApplicationDbContext context, IApplicationCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task Handle(ProcessOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, order);

        order.Process();

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, cancellationToken);
    }
}
