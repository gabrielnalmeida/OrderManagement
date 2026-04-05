using OrderManagement.Application.Common.Interfaces;

namespace OrderManagement.Application.Products.Queries.GetProducts;

public record GetProductsQuery : IRequest<IReadOnlyCollection<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyCollection<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
       return await _context.Products
        .AsNoTracking()
        .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
        .OrderBy(x => x.Name)
        .ToListAsync(cancellationToken);
    }
}
