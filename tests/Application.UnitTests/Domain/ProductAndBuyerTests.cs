namespace OrderManagement.Application.UnitTests.Domain;

public class ProductTests
{
    [Test]
    public void ShouldCreateProduct()
    {
        var product = new Product("Notebook", "Desc", 10m);

        product.Name.ShouldBe("Notebook");
        product.Description.ShouldBe("Desc");
        product.Price.ShouldBe(10m);
    }

    [Test]
    public void ShouldUpdateProduct()
    {
        var product = new Product("Notebook", "Desc", 10m);

        product.Update("Mouse", "Novo", 20m);

        product.Name.ShouldBe("Mouse");
        product.Description.ShouldBe("Novo");
        product.Price.ShouldBe(20m);
    }
}

public class BuyerTests
{
    [Test]
    public void ShouldCreateBuyer()
    {
        var buyer = new Buyer("Alice");

        buyer.Name.ShouldBe("Alice");
        buyer.Orders.ShouldBeEmpty();
    }

    [Test]
    public void ShouldUpdateBuyer()
    {
        var buyer = new Buyer("Alice");

        buyer.Update("Bob");

        buyer.Name.ShouldBe("Bob");
    }
}
