using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.Orders.Commands.DeleteOrder;
using OrderManagement.Application.UnitTests.Common;

namespace OrderManagement.Application.UnitTests.Handlers;

public class CreateOrderCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenBuyerDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db =>
        {
            db.Products.Add(new Product("Product", null, 10m));
        });

        var cache = new Mock<IApplicationCache>();
        var handler = new CreateOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new CreateOrderCommand
        {
            BuyerId = 999,
            Items = [new SlimOrderItemDto { ProductId = context.Products.Single().Id, Quantity = 1 }]
        }, CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldThrowValidationExceptionWhenAnyProductDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db =>
        {
            db.Buyers.Add(new Buyer("Buyer"));
            db.Products.Add(new Product("Product", null, 10m));
        });

        var cache = new Mock<IApplicationCache>();
        var handler = new CreateOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new CreateOrderCommand
        {
            BuyerId = context.Buyers.Single().Id,
            Items =
            [
                new SlimOrderItemDto { ProductId = context.Products.Single().Id, Quantity = 1 },
                new SlimOrderItemDto { ProductId = 999, Quantity = 1 }
            ]
        }, CancellationToken.None);

        var exception = await action.ShouldThrowAsync<OrderManagement.Application.Common.Exceptions.ValidationException>();
        exception.Errors.ShouldContainKey("Items");
    }

    [Test]
    public async Task ShouldCreateOrderWithCurrentProductPrices()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db =>
        {
            db.Buyers.Add(new Buyer("Buyer"));
            db.Products.AddRange(
                new Product("Product 1", null, 10m),
                new Product("Product 2", null, 4m));
        });

        var cache = new Mock<IApplicationCache>();
        var handler = new CreateOrderCommandHandler(context, cache.Object);

        var orderId = await handler.Handle(new CreateOrderCommand
        {
            BuyerId = context.Buyers.Single().Id,
            Items =
            [
                new SlimOrderItemDto { ProductId = context.Products.OrderBy(x => x.Id).First().Id, Quantity = 2 },
                new SlimOrderItemDto { ProductId = context.Products.OrderBy(x => x.Id).Last().Id, Quantity = 3 }
            ]
        }, CancellationToken.None);

        var order = await context.Orders.Include(x => x.Items).SingleAsync(x => x.Id == orderId);
        order.Status.ShouldBe(OrderStatus.Started);
        order.Items.Select(x => x.UnitPrice).OrderBy(x => x).ShouldBe([4m, 10m]);
    }

    [Test]
    public async Task ShouldSaveChangesAndInvalidateOrdersListCacheWhenOrderIsCreated()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db =>
        {
            db.Buyers.Add(new Buyer("Buyer"));
            db.Products.Add(new Product("Product", null, 10m));
        });

        var cache = new Mock<IApplicationCache>();
        var handler = new CreateOrderCommandHandler(context, cache.Object);

        await handler.Handle(new CreateOrderCommand
        {
            BuyerId = context.Buyers.Single().Id,
            Items = [new SlimOrderItemDto { ProductId = context.Products.Single().Id, Quantity = 1 }]
        }, CancellationToken.None);

        context.Orders.Count().ShouldBe(1);
        cache.Verify(x => x.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class UpdateOrderCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync();
        var cache = new Mock<IApplicationCache>();
        var handler = new UpdateOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new UpdateOrderCommand { Id = 999, Items = [new SlimOrderItemDto { ProductId = 1, Quantity = 1 }] }, CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldThrowValidationExceptionWhenAnyProductDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: SeedStartedOrder);
        var cache = new Mock<IApplicationCache>();
        var handler = new UpdateOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new UpdateOrderCommand
        {
            Id = context.Orders.Single().Id,
            Items = [new SlimOrderItemDto { ProductId = 999, Quantity = 1 }]
        }, CancellationToken.None);

        await action.ShouldThrowAsync<OrderManagement.Application.Common.Exceptions.ValidationException>();
    }

    [Test]
    public async Task ShouldReplaceItemsWhenOrderIsStarted()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db =>
        {
            SeedStartedOrder(db);
            db.Products.Add(new Product("Updated", null, 20m));
        });

        var cache = new Mock<IApplicationCache>();
        var handler = new UpdateOrderCommandHandler(context, cache.Object);
        var orderId = context.Orders.Single().Id;
        var newProductId = context.Products.OrderBy(x => x.Id).Last().Id;

        await handler.Handle(new UpdateOrderCommand
        {
            Id = orderId,
            Items = [new SlimOrderItemDto { ProductId = newProductId, Quantity = 5 }]
        }, CancellationToken.None);

        var updatedOrder = await context.Orders.Include(x => x.Items).SingleAsync(x => x.Id == orderId);
        updatedOrder.Items.Count.ShouldBe(1);
        updatedOrder.Items.Single().ProductId.ShouldBe(newProductId);
    }

    [Test]
    public async Task ShouldThrowValidationExceptionWhenOrderCannotBeChanged()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db =>
        {
            SeedStartedOrder(db);
            db.SaveChanges();
            db.Orders.Single().Process();
        });

        var cache = new Mock<IApplicationCache>();
        var handler = new UpdateOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new UpdateOrderCommand
        {
            Id = context.Orders.Single().Id,
            Items = [new SlimOrderItemDto { ProductId = context.Products.Single().Id, Quantity = 2 }]
        }, CancellationToken.None);

        await action.ShouldThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
    }

    [Test]
    public async Task ShouldSaveChangesAndInvalidateOrdersListCacheWhenOrderIsUpdated()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: SeedStartedOrder);
        var cache = new Mock<IApplicationCache>();
        var handler = new UpdateOrderCommandHandler(context, cache.Object);

        await handler.Handle(new UpdateOrderCommand
        {
            Id = context.Orders.Single().Id,
            Items = [new SlimOrderItemDto { ProductId = context.Products.Single().Id, Quantity = 2 }]
        }, CancellationToken.None);

        context.Orders.Include(x => x.Items).Single().Items.Single().Quantity.ShouldBe(2);
        cache.Verify(x => x.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SeedStartedOrder(ApplicationDbContext db)
    {
        var buyer = new Buyer("Buyer");
        var product = new Product("Product", null, 10m);
        db.Buyers.Add(buyer);
        db.Products.Add(product);
        db.SaveChanges();
        db.Orders.Add(new Order(buyer.Id, [new OrderItem(product.Id, 1, product.Price)]));
    }
}

public class DeleteOrderCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync();
        var cache = new Mock<IApplicationCache>();
        var handler = new DeleteOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new DeleteOrderCommand(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRemoveOrderAndInvalidateOrdersListCacheWhenOrderExists()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: SeedOrder);
        var cache = new Mock<IApplicationCache>();
        var handler = new DeleteOrderCommandHandler(context, cache.Object);

        await handler.Handle(new DeleteOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        context.Orders.ShouldBeEmpty();
        cache.Verify(x => x.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldSaveChangesWhenOrderIsDeleted()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: SeedOrder);
        var cache = new Mock<IApplicationCache>();
        var handler = new DeleteOrderCommandHandler(context, cache.Object);

        await handler.Handle(new DeleteOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        (await context.Orders.CountAsync()).ShouldBe(0);
    }

    private static void SeedOrder(ApplicationDbContext db)
    {
        var buyer = new Buyer("Buyer");
        var product = new Product("Product", null, 10m);
        db.Buyers.Add(buyer);
        db.Products.Add(product);
        db.SaveChanges();
        db.Orders.Add(new Order(buyer.Id, [new OrderItem(product.Id, 1, product.Price)]));
    }
}

public class CancelOrderCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync();
        var cache = new Mock<IApplicationCache>();
        var handler = new CancelOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new CancelOrderCommand(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [TestCase(OrderStatus.Started)]
    [TestCase(OrderStatus.Processed)]
    public async Task ShouldCancelOrderWhenAllowed(OrderStatus status)
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, status));
        var cache = new Mock<IApplicationCache>();
        var handler = new CancelOrderCommandHandler(context, cache.Object);

        await handler.Handle(new CancelOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        context.Orders.Single().Status.ShouldBe(OrderStatus.Cancelled);
        cache.Verify(x => x.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldSaveChangesWhenOrderIsCancelled()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, OrderStatus.Started));
        var cache = new Mock<IApplicationCache>();
        var handler = new CancelOrderCommandHandler(context, cache.Object);

        await handler.Handle(new CancelOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        (await context.Orders.SingleAsync()).Status.ShouldBe(OrderStatus.Cancelled);
    }

    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task ShouldThrowValidationExceptionWhenOrderCannotBeCancelled(OrderStatus status)
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, status));
        var cache = new Mock<IApplicationCache>();
        var handler = new CancelOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new CancelOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        await action.ShouldThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
    }

    private static void SeedOrderWithStatus(ApplicationDbContext db, OrderStatus status)
    {
        var buyer = new Buyer("Buyer");
        var product = new Product("Product", null, 10m);
        db.Buyers.Add(buyer);
        db.Products.Add(product);
        db.SaveChanges();

        var order = new Order(buyer.Id, [new OrderItem(product.Id, 1, product.Price)]);
        order.UpdateStatus(status);

        db.Orders.Add(order);
    }
}

public class ProcessOrderCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync();
        var cache = new Mock<IApplicationCache>();
        var handler = new ProcessOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new ProcessOrderCommand(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldProcessStartedOrderAndInvalidateCache()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, OrderStatus.Started));
        var cache = new Mock<IApplicationCache>();
        var handler = new ProcessOrderCommandHandler(context, cache.Object);

        await handler.Handle(new ProcessOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        context.Orders.Single().Status.ShouldBe(OrderStatus.Processed);
        cache.Verify(x => x.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldSaveChangesWhenOrderIsProcessed()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, OrderStatus.Started));
        var cache = new Mock<IApplicationCache>();
        var handler = new ProcessOrderCommandHandler(context, cache.Object);

        await handler.Handle(new ProcessOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        (await context.Orders.SingleAsync()).Status.ShouldBe(OrderStatus.Processed);
    }

    [TestCase(OrderStatus.Processed)]
    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task ShouldThrowValidationExceptionWhenOrderCannotBeProcessed(OrderStatus status)
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, status));
        var cache = new Mock<IApplicationCache>();
        var handler = new ProcessOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new ProcessOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        await action.ShouldThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
    }

    private static void SeedOrderWithStatus(ApplicationDbContext db, OrderStatus status)
    {
        var buyer = new Buyer("Buyer");
        var product = new Product("Product", null, 10m);
        db.Buyers.Add(buyer);
        db.Products.Add(product);
        db.SaveChanges();

        var order = new Order(buyer.Id, [new OrderItem(product.Id, 1, product.Price)]);
        order.UpdateStatus(status);

        db.Orders.Add(order);
    }
}

public class ShipOrderCommandHandlerTests
{
    [Test]
    public async Task ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync();
        var cache = new Mock<IApplicationCache>();
        var handler = new ShipOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new ShipOrderCommand(999), CancellationToken.None);

        await action.ShouldThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldShipProcessedOrderAndInvalidateCache()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, OrderStatus.Processed));
        var cache = new Mock<IApplicationCache>();
        var handler = new ShipOrderCommandHandler(context, cache.Object);

        await handler.Handle(new ShipOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        context.Orders.Single().Status.ShouldBe(OrderStatus.Shipped);
        cache.Verify(x => x.RemoveByPrefixAsync(CachingKeys.OrdersListPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldSaveChangesWhenOrderIsShipped()
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, OrderStatus.Processed));
        var cache = new Mock<IApplicationCache>();
        var handler = new ShipOrderCommandHandler(context, cache.Object);

        await handler.Handle(new ShipOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        (await context.Orders.SingleAsync()).Status.ShouldBe(OrderStatus.Shipped);
    }

    [TestCase(OrderStatus.Started)]
    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task ShouldThrowValidationExceptionWhenOrderCannotBeShipped(OrderStatus status)
    {
        await using var context = await TestDbContextFactory.CreateContextAsync(seed: db => SeedOrderWithStatus(db, status));
        var cache = new Mock<IApplicationCache>();
        var handler = new ShipOrderCommandHandler(context, cache.Object);

        var action = async () => await handler.Handle(new ShipOrderCommand(context.Orders.Single().Id), CancellationToken.None);

        await action.ShouldThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
    }

    private static void SeedOrderWithStatus(ApplicationDbContext db, OrderStatus status)
    {
        var buyer = new Buyer("Buyer");
        var product = new Product("Product", null, 10m);
        db.Buyers.Add(buyer);
        db.Products.Add(product);
        db.SaveChanges();

        var order = new Order(buyer.Id, [new OrderItem(product.Id, 1, product.Price)]);
        order.UpdateStatus(status);

        db.Orders.Add(order);
    }
}
