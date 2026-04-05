using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Buyer> Buyers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
