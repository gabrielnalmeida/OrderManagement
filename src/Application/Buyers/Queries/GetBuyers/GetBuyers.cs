using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Buyers.Queries.GetBuyers;

public record GetBuyersQuery : IRequest<IReadOnlyCollection<BuyerDto>>;

public class GetBuyersQueryHandler : IRequestHandler<GetBuyersQuery, IReadOnlyCollection<BuyerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBuyersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<BuyerDto>> Handle(GetBuyersQuery request, CancellationToken cancellationToken)
    {
       return await _context.Buyers
        .AsNoTracking()
        .ProjectTo<BuyerDto>(_mapper.ConfigurationProvider)
        .OrderBy(x => x.Name)
        .ToListAsync(cancellationToken);
    }
}
