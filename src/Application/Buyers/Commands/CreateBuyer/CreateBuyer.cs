using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Buyers.Commands.CreateBuyer;

public record CreateBuyerCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
}

public class CreateBuyerCommandHandler : IRequestHandler<CreateBuyerCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateBuyerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
    {
        var entity = new Buyer(request.Name);

        _context.Buyers.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
