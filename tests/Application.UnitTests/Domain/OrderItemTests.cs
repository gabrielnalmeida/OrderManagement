namespace OrderManagement.Application.UnitTests.Domain;

public class OrderItemTests
{
    [Test]
    public void ShouldCreateOrderItemWhenValuesAreValid()
    {
        var item = new OrderItem(4, 2, 12.5m);

        item.ProductId.ShouldBe(4);
        item.Quantity.ShouldBe(2);
        item.UnitPrice.ShouldBe(12.5m);
    }

    [Test]
    public void ShouldCalculateSubtotal()
    {
        var item = new OrderItem(4, 3, 12.5m);

        item.GetSubTotal().ShouldBe(37.5m);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldThrowWhenQuantityIsZeroOrLess(int quantity)
    {
        var action = () => new OrderItem(4, quantity, 10m);

        action.ShouldThrow<System.ComponentModel.DataAnnotations.ValidationException>()
            .Message.ShouldBe("A quantidade deve ser maior que zero.");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldThrowWhenUnitPriceIsZeroOrLess(decimal unitPrice)
    {
        var action = () => new OrderItem(4, 1, unitPrice);

        action.ShouldThrow<System.ComponentModel.DataAnnotations.ValidationException>()
            .Message.ShouldBe("O preço unitário deve ser maior que zero.");
    }
}
