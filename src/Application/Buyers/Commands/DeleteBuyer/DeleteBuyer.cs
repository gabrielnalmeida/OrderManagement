using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Buyers.Commands.DeleteBuyer;

public record DeleteBuyerCommand(int Id) : IRequest;

public class DeleteBuyerCommandHandler : IRequestHandler<DeleteBuyerCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteBuyerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteBuyerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Buyers
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Buyers.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

}
