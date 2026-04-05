using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery (int Id): IRequest<OrderDetailsDto>;

    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDetailsDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<OrderDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, order);

        return order;
    }
}
