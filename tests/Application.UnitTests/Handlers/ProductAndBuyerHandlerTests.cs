using OrderManagement.Application.UnitTests.Common;

namespace OrderManagement.Application.UnitTests.Handlers;

public class CreateProductCommandHandlerTests
{
    [Test]
    public async Task ShouldCreateProductAndSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new CreateProductCommandHandler(context);

        var id = await handler.Handle(new CreateProductCommand { Name = "Product", Description = "Desc", Price = 10m }, CancellationToken.None);

        var product = await context.Products.SingleAsync(x => x.Id == id);
        product.Name.ShouldBe("Product");
        product.Price.ShouldBe(10m);
    }

    [Test]
    public async Task ShouldSaveChangesWhenProductIsCreated()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new CreateProductCommandHandler(context);

        await handler.Handle(new CreateProductCommand { Name = "Product", Description = "Desc", Price = 10m }, CancellationToken.None);

        context.Products.Count().ShouldBe(1);
    }
}

public class UpdateProductCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenProductDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new UpdateProductCommandHandler(context);

        var action = async () => await handler.Handle(new UpdateProductCommand { Id = 999, Name = "Updated", Price = 10m }, CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateProductWhenItExists()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(db => db.Products.Add(new Product("Original", null, 1m)));
        var handler = new UpdateProductCommandHandler(context);
        var id = context.Products.Single().Id;

        await handler.Handle(new UpdateProductCommand { Id = id, Name = "Updated", Description = "Desc", Price = 10m }, CancellationToken.None);

        var product = await context.Products.SingleAsync();
        product.Name.ShouldBe("Updated");
        product.Description.ShouldBe("Desc");
        product.Price.ShouldBe(10m);
    }
}

public class DeleteProductCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenProductDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new DeleteProductCommandHandler(context);

        var action = async () => await handler.Handle(new DeleteProductCommand(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRemoveProductWhenItExists()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(db => db.Products.Add(new Product("Original", null, 1m)));
        var handler = new DeleteProductCommandHandler(context);
        var id = context.Products.Single().Id;

        await handler.Handle(new DeleteProductCommand(id), CancellationToken.None);

        context.Products.ShouldBeEmpty();
    }
}

public class GetProductsQueryHandlerTests
{
    [Test]
    public async Task ShouldReturnProductsOrderedByName()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(db =>
        {
            db.Products.AddRange(
                new Product("Mouse", null, 5m),
                new Product("Keyboard", null, 10m));
        });

        var handler = new GetProductsQueryHandler(context, TestMapperFactory.Instance);

        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        result.Select(x => x.Name).ShouldBe(["Keyboard", "Mouse"]);
    }
}

public class GetProductByIdQueryHandlerTests
{
    [Test]
    public async Task ShouldReturnProductWhenItExists()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(db => db.Products.Add(new Product("Mouse", null, 5m)));
        var handler = new GetProductByIdQueryHandler(context, TestMapperFactory.Instance);
        var id = context.Products.Single().Id;

        var result = await handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        result.Name.ShouldBe("Mouse");
    }

    [Test]
    public async Task ShouldThrowNotFoundWhenProductDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new GetProductByIdQueryHandler(context, TestMapperFactory.Instance);

        var action = async () => await handler.Handle(new GetProductByIdQuery(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }
}

public class CreateBuyerCommandHandlerTests
{
    [Test]
    public async Task ShouldCreateBuyerAndSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new CreateBuyerCommandHandler(context);

        var id = await handler.Handle(new CreateBuyerCommand { Name = "Buyer" }, CancellationToken.None);

        (await context.Buyers.SingleAsync(x => x.Id == id)).Name.ShouldBe("Buyer");
    }

    [Test]
    public async Task ShouldSaveChangesWhenBuyerIsCreated()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new CreateBuyerCommandHandler(context);

        await handler.Handle(new CreateBuyerCommand { Name = "Buyer" }, CancellationToken.None);

        context.Buyers.Count().ShouldBe(1);
    }
}

public class UpdateBuyerCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenBuyerDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new UpdateBuyerCommandHandler(context);

        var action = async () => await handler.Handle(new UpdateBuyerCommand { Id = 999, Name = "Updated" }, CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldUpdateBuyerWhenItExists()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(db => db.Buyers.Add(new Buyer("Buyer")));
        var handler = new UpdateBuyerCommandHandler(context);
        var id = context.Buyers.Single().Id;

        await handler.Handle(new UpdateBuyerCommand { Id = id, Name = "Updated" }, CancellationToken.None);

        context.Buyers.Single().Name.ShouldBe("Updated");
    }
}

public class DeleteBuyerCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenBuyerDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var handler = new DeleteBuyerCommandHandler(context);

        var action = async () => await handler.Handle(new DeleteBuyerCommand(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRemoveBuyerWhenItExists()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(db => db.Buyers.Add(new Buyer("Buyer")));
        var handler = new DeleteBuyerCommandHandler(context);
        var id = context.Buyers.Single().Id;

        await handler.Handle(new DeleteBuyerCommand(id), CancellationToken.None);

        context.Buyers.ShouldBeEmpty();
    }
}

public class GetBuyersQueryHandlerTests
{
    [Test]
    public async Task ShouldReturnBuyersOrderedByName()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(db =>
        {
            db.Buyers.AddRange(new Buyer("Charlie"), new Buyer("Alice"));
        });

        var handler = new GetBuyersQueryHandler(context, TestMapperFactory.Instance);

        var result = await handler.Handle(new GetBuyersQuery(), CancellationToken.None);

        result.Select(x => x.Name).ShouldBe(["Alice", "Charlie"]);
    }
}
