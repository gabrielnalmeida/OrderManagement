using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(int Id) : IRequest;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Orders
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Orders.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

}
