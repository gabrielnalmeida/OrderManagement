using FluentValidation.Results;
using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Common.Interfaces;
using DomainValidationException = System.ComponentModel.DataAnnotations.ValidationException;
using AppValidationException = OrderManagement.Application.Common.Exceptions.ValidationException;

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

        try
        {
            order.Ship();
        }
        catch (DomainValidationException ex)
        {
            throw new AppValidationException(
                [new ValidationFailure(nameof(order.Status), ex.Message)]
            );
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, cancellationToken);
    }
}
