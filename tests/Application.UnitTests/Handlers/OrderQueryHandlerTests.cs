using OrderManagement.Application.UnitTests.Common;

namespace OrderManagement.Application.UnitTests.Handlers;

public class GetOrdersQueryHandlerTests
{
    [Test]
    public async Task ShouldReturnAllOrdersWhenNoFilterIsProvided()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrders);
        var handler = new GetOrdersQueryHandler(context, TestMapperFactory.Instance);

        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        result.Count.ShouldBe(3);
    }

    [Test]
    public async Task ShouldFilterByBuyerId()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrders);
        var handler = new GetOrdersQueryHandler(context, TestMapperFactory.Instance);
        var buyerId = context.Buyers.OrderBy(x => x.Id).First().Id;

        var result = await handler.Handle(new GetOrdersQuery { BuyerId = buyerId }, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.All(x => x.BuyerId == buyerId).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldFilterByStatus()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrders);
        var handler = new GetOrdersQueryHandler(context, TestMapperFactory.Instance);

        var result = await handler.Handle(new GetOrdersQuery { Status = OrderStatus.Processed }, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(OrderStatus.Processed);
    }

    [Test]
    public async Task ShouldFilterByCreatedFrom()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrders);
        var handler = new GetOrdersQueryHandler(context, TestMapperFactory.Instance);
        var createdFrom = context.Orders.OrderBy(x => x.Created).Select(x => x.Created).ToArray()[1];

        var result = await handler.Handle(new GetOrdersQuery
        {
            CreatedFrom = createdFrom.UtcDateTime
        }, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.All(x => x.Created >= createdFrom).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldFilterByCreatedTo()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrders);
        var handler = new GetOrdersQueryHandler(context, TestMapperFactory.Instance);
        var createdTo = context.Orders.OrderBy(x => x.Created).Select(x => x.Created).ToArray()[1];

        var result = await handler.Handle(new GetOrdersQuery
        {
            CreatedTo = createdTo.UtcDateTime
        }, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.All(x => x.Created <= createdTo).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldFilterByCreatedRange()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrders);
        var handler = new GetOrdersQueryHandler(context, TestMapperFactory.Instance);
        var createdDates = context.Orders.OrderBy(x => x.Created).Select(x => x.Created).ToArray();

        var result = await handler.Handle(new GetOrdersQuery
        {
            CreatedFrom = createdDates[1].UtcDateTime,
            CreatedTo = createdDates[2].UtcDateTime
        }, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(context.Orders.OrderBy(x => x.Created).Skip(1).Select(x => x.Id).ToList());
    }

    [Test]
    public async Task ShouldOrderByCreatedDateAndMapTotals()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrders);
        var handler = new GetOrdersQueryHandler(context, TestMapperFactory.Instance);

        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        result.Select(x => x.Id).ShouldBe(context.Orders.OrderBy(x => x.Created).Select(x => x.Id).ToList());
        result.First().BuyerName.ShouldNotBeNullOrWhiteSpace();
        result.First().TotalItems.ShouldBeGreaterThan(0);
        result.First().TotalAmount.ShouldBeGreaterThan(0);
    }

    private static void SeedOrders(ApplicationDbContext db)
    {
        var buyer1 = new Buyer("Alice");
        var buyer2 = new Buyer("Bob");
        var product1 = new Product("Keyboard", null, 10m);
        var product2 = new Product("Mouse", null, 20m);

        db.Buyers.AddRange(buyer1, buyer2);
        db.Products.AddRange(product1, product2);
        db.SaveChanges();

        var order1 = new Order(buyer1.Id, [new OrderItem(product1.Id, 2, product1.Price)])
        {
            Created = new DateTimeOffset(2026, 4, 1, 8, 0, 0, TimeSpan.Zero)
        };

        var order2 = new Order(buyer2.Id, [new OrderItem(product2.Id, 1, product2.Price)])
        {
            Created = new DateTimeOffset(2026, 4, 2, 8, 0, 0, TimeSpan.Zero)
        };
        order2.Process();

        var order3 = new Order(buyer1.Id,
        [
            new OrderItem(product1.Id, 1, product1.Price),
            new OrderItem(product2.Id, 1, product2.Price)
        ])
        {
            Created = new DateTimeOffset(2026, 4, 3, 8, 0, 0, TimeSpan.Zero)
        };

        db.Orders.AddRange(order1, order2, order3);
    }
}

public class GetOrderByIdQueryHandlerTests
{
    [Test]
    public async Task ShouldReturnOrderDetailsWhenOrderExists()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrder);
        var handler = new GetOrderByIdQueryHandler(context, TestMapperFactory.Instance);
        var order = context.Orders.Single();

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.Id.ShouldBe(order.Id);
        result.Items.Count.ShouldBe(2);
        result.TotalAmount.ShouldBe(40m);
    }

    [Test]
    public async Task ShouldMapItemsAndTotalAmount()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(SeedOrder);
        var handler = new GetOrderByIdQueryHandler(context, TestMapperFactory.Instance);
        var order = context.Orders.Single();

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.Items.Select(x => x.ProductName).OrderBy(x => x).ShouldBe(["Keyboard", "Mouse"]);
        result.Items.Select(x => x.Subtotal).OrderBy(x => x).ShouldBe([20m, 20m]);
        result.TotalAmount.ShouldBe(40m);
    }

    [Test]
    public async Task ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync();
        var handler = new GetOrderByIdQueryHandler(context, TestMapperFactory.Instance);

        var action = async () => await handler.Handle(new GetOrderByIdQuery(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    private static void SeedOrder(ApplicationDbContext db)
    {
        var buyer = new Buyer("Alice");
        var product1 = new Product("Keyboard", null, 10m);
        var product2 = new Product("Mouse", null, 20m);

        db.Buyers.Add(buyer);
        db.Products.AddRange(product1, product2);
        db.SaveChanges();

        db.Orders.Add(new Order(buyer.Id,
        [
            new OrderItem(product1.Id, 2, product1.Price),
            new OrderItem(product2.Id, 1, product2.Price)
        ]));
    }
}
