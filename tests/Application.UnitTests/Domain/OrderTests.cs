namespace OrderManagement.Application.UnitTests.Domain;

public class OrderTests
{
    [Test]
    public void ShouldCreateOrderWithStartedStatus()
    {
        var order = CreateOrder();

        order.Status.ShouldBe(OrderStatus.Started);
        order.BuyerId.ShouldBe(1);
        order.Items.Count.ShouldBe(1);
    }

    [Test]
    public void ShouldCreateOrderWithBuyerIdAndItems()
    {
        var items = new[]
        {
            new OrderItem(10, 2, 5m),
            new OrderItem(11, 1, 7m)
        };

        var order = new Order(42, items);

        order.BuyerId.ShouldBe(42);
        order.Items.Count.ShouldBe(2);
        order.Items.Select(x => x.ProductId).ShouldBe([10, 11]);
    }

    [Test]
    public void ShouldCalculateTotalFromItems()
    {
        var order = new Order(1,
        [
            new OrderItem(10, 2, 5m),
            new OrderItem(11, 3, 4m)
        ]);

        order.GetTotal().ShouldBe(22m);
    }

    [Test]
    public void ShouldAllowReplaceItemsWhenStatusIsStarted()
    {
        var order = CreateOrder();

        order.ReplaceItems([new OrderItem(2, 4, 2.5m)]);

        order.Items.Count.ShouldBe(1);
        order.Items.Single().ProductId.ShouldBe(2);
    }

    [Test]
    public void ShouldNotAllowReplaceItemsWhenItemsAreEmpty()
    {
        var order = CreateOrder();

        var action = () => order.ReplaceItems([]);

        action.ShouldThrow<System.ComponentModel.DataAnnotations.ValidationException>()
            .Message.ShouldBe("Um pedido deve conter pelo menos um item.");
    }

    [TestCase(OrderStatus.Processed)]
    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Cancelled)]
    public void ShouldNotAllowReplaceItemsWhenStatusIsNotStarted(OrderStatus status)
    {
        var order = CreateOrder();
        order.UpdateStatus(status);

        var action = () => order.ReplaceItems([new OrderItem(2, 1, 1m)]);

        action.ShouldThrow<System.ComponentModel.DataAnnotations.ValidationException>()
            .Message.ShouldBe("Apenas pedidos não processados podem ser alterados.");
    }

    [TestCase(OrderStatus.Started, true)]
    [TestCase(OrderStatus.Processed, false)]
    [TestCase(OrderStatus.Shipped, false)]
    [TestCase(OrderStatus.Cancelled, false)]
    public void ShouldReturnWhetherOrderCanBeChanged(OrderStatus status, bool expected)
    {
        var order = CreateOrder();
        order.UpdateStatus(status);

        order.CanBeChanged().ShouldBe(expected);
    }

    [TestCase(OrderStatus.Started)]
    [TestCase(OrderStatus.Processed)]
    public void ShouldCancelOrderWhenStatusAllows(OrderStatus status)
    {
        var order = CreateOrder();
        order.UpdateStatus(status);

        order.Cancel();

        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Cancelled)]
    public void ShouldNotCancelOrderWhenStatusDoesNotAllow(OrderStatus status)
    {
        var order = CreateOrder();
        order.UpdateStatus(status);

        var action = () => order.Cancel();

        action.ShouldThrow<System.ComponentModel.DataAnnotations.ValidationException>()
            .Message.ShouldBe("Apenas pedidos iniciados ou processados podem ser cancelados.");
    }

    [Test]
    public void ShouldProcessOrderWhenStarted()
    {
        var order = CreateOrder();

        order.Process();

        order.Status.ShouldBe(OrderStatus.Processed);
    }

    [TestCase(OrderStatus.Processed)]
    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Cancelled)]
    public void ShouldNotProcessOrderWhenStatusIsNotStarted(OrderStatus status)
    {
        var order = CreateOrder();
        order.UpdateStatus(status);

        var action = () => order.Process();

        action.ShouldThrow<System.ComponentModel.DataAnnotations.ValidationException>()
            .Message.ShouldBe("Apenas pedidos iniciados podem ser processados.");
    }

    [Test]
    public void ShouldShipOrderWhenProcessed()
    {
        var order = CreateOrder();
        order.Process();

        order.Ship();

        order.Status.ShouldBe(OrderStatus.Shipped);
    }

    [TestCase(OrderStatus.Started)]
    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Cancelled)]
    public void ShouldNotShipOrderWhenStatusIsNotProcessed(OrderStatus status)
    {
        var order = CreateOrder();
        order.UpdateStatus(status);

        var action = () => order.Ship();

        action.ShouldThrow<System.ComponentModel.DataAnnotations.ValidationException>()
            .Message.ShouldBe("Apenas pedidos processados podem ser enviados.");
    }

    private static Order CreateOrder()
    {
        return new Order(1, [new OrderItem(1, 2, 10m)]);
    }
}
