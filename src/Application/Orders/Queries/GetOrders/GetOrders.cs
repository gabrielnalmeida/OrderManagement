using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery : IRequest<IReadOnlyCollection<OrderSummaryDto>>, ICacheableQuery
{
    public int? BuyerId { get; init; }
    public OrderStatus? Status { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }

    public string CacheKey => CachingKeys.GetOrdersListCacheKey(BuyerId, Status?.ToString(), CreatedFrom, CreatedTo);

    public TimeSpan Expiration => TimeSpan.FromMinutes(2);
}

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyCollection<OrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<OrderSummaryDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .AsNoTracking()
            .AsQueryable();

        if (request.BuyerId.HasValue)
        {
            query = query.Where(x => x.BuyerId == request.BuyerId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(x => x.Created >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(x => x.Created <= request.CreatedTo.Value);
        }

        return await query
            .OrderBy(x => x.Created)
            .ProjectTo<OrderSummaryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
