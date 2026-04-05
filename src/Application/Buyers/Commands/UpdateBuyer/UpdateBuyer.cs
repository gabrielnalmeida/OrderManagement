using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Buyers.Commands.UpdateBuyer;

public record UpdateBuyerCommand : IRequest
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class UpdateBuyerCommandHandler : IRequestHandler<UpdateBuyerCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateBuyerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateBuyerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Buyers
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Update(request.Name);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
