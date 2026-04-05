using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OrderManagement.Application.UnitTests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(static warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    public static async Task<ApplicationDbContext> CreateContextAsync(Action<ApplicationDbContext>? seed = null)
    {
        var context = CreateContext();
        seed?.Invoke(context);
        await context.SaveChangesAsync(CancellationToken.None);
        return context;
    }
}
