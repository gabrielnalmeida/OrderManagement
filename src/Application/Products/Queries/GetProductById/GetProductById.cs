using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Products.Queries.GetProducts;

namespace OrderManagement.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
                .AsNoTracking()
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, product);

        return product;
    }
}
