namespace OrderManagement.Application.UnitTests.Validators;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Test]
    public void ShouldFailWhenBuyerIdIsEmpty()
    {
        var result = _validator.Validate(new CreateOrderCommand { BuyerId = 0, Items = [new SlimOrderItemDto { ProductId = 1, Quantity = 1 }] });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "BuyerId");
    }

    [Test]
    public void ShouldFailWhenItemsAreEmpty()
    {
        var result = _validator.Validate(new CreateOrderCommand { BuyerId = 1, Items = [] });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Items");
    }

    [Test]
    public void ShouldPassWhenCommandIsValid()
    {
        var result = _validator.Validate(new CreateOrderCommand { BuyerId = 1, Items = [new SlimOrderItemDto { ProductId = 1, Quantity = 1 }] });

        result.IsValid.ShouldBeTrue();
    }
}

public class UpdateOrderCommandValidatorTests
{
    private readonly UpdateOrderCommandValidator _validator = new();

    [Test]
    public void ShouldFailWhenIdIsInvalid()
    {
        var result = _validator.Validate(new UpdateOrderCommand { Id = 0, Items = [new SlimOrderItemDto { ProductId = 1, Quantity = 1 }] });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Id");
    }

    [Test]
    public void ShouldFailWhenItemsAreEmpty()
    {
        var result = _validator.Validate(new UpdateOrderCommand { Id = 1, Items = [] });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Items");
    }

    [Test]
    public void ShouldPassWhenCommandIsValid()
    {
        var result = _validator.Validate(new UpdateOrderCommand { Id = 1, Items = [new SlimOrderItemDto { ProductId = 1, Quantity = 1 }] });

        result.IsValid.ShouldBeTrue();
    }
}

public class SlimOrderItemDtoValidatorTests
{
    private readonly SlimOrderItemDtoValidator _validator = new();

    [Test]
    public void ShouldFailWhenProductIdIsEmpty()
    {
        var result = _validator.Validate(new SlimOrderItemDto { ProductId = 0, Quantity = 1 });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "ProductId");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldFailWhenQuantityIsZeroOrLess(int quantity)
    {
        var result = _validator.Validate(new SlimOrderItemDto { ProductId = 1, Quantity = quantity });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Quantity");
    }

    [Test]
    public void ShouldPassWhenItemIsValid()
    {
        var result = _validator.Validate(new SlimOrderItemDto { ProductId = 1, Quantity = 1 });

        result.IsValid.ShouldBeTrue();
    }
}

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Test]
    public void ShouldFailWhenNameIsEmpty()
    {
        _validator.Validate(new CreateProductCommand { Name = string.Empty, Price = 1m }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailWhenNameExceedsLimit()
    {
        _validator.Validate(new CreateProductCommand { Name = new string('a', 101), Price = 1m }).Errors
            .ShouldContain(x => x.PropertyName == "Name");
    }

    [Test]
    public void ShouldFailWhenDescriptionExceedsLimit()
    {
        _validator.Validate(new CreateProductCommand { Name = "Ok", Description = new string('a', 501), Price = 1m }).Errors
            .ShouldContain(x => x.PropertyName == "Description");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldFailWhenPriceIsZeroOrLess(decimal price)
    {
        _validator.Validate(new CreateProductCommand { Name = "Ok", Price = price }).Errors
            .ShouldContain(x => x.PropertyName == "Price");
    }

    [Test]
    public void ShouldPassWhenCommandIsValid()
    {
        _validator.Validate(new CreateProductCommand { Name = "Ok", Description = "Desc", Price = 1m }).IsValid.ShouldBeTrue();
    }
}

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Test]
    public void ShouldFailWhenNameIsEmpty()
    {
        _validator.Validate(new UpdateProductCommand { Id = 1, Name = string.Empty, Price = 1m }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailWhenNameExceedsLimit()
    {
        _validator.Validate(new UpdateProductCommand { Id = 1, Name = new string('a', 101), Price = 1m }).Errors
            .ShouldContain(x => x.PropertyName == "Name");
    }

    [Test]
    public void ShouldFailWhenDescriptionExceedsLimit()
    {
        _validator.Validate(new UpdateProductCommand { Id = 1, Name = "Ok", Description = new string('a', 501), Price = 1m }).Errors
            .ShouldContain(x => x.PropertyName == "Description");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ShouldFailWhenPriceIsZeroOrLess(decimal price)
    {
        _validator.Validate(new UpdateProductCommand { Id = 1, Name = "Ok", Price = price }).Errors
            .ShouldContain(x => x.PropertyName == "Price");
    }

    [Test]
    public void ShouldPassWhenCommandIsValid()
    {
        _validator.Validate(new UpdateProductCommand { Id = 1, Name = "Ok", Description = "Desc", Price = 1m }).IsValid.ShouldBeTrue();
    }
}

public class CreateBuyerCommandValidatorTests
{
    private readonly CreateBuyerCommandValidator _validator = new();

    [Test]
    public void ShouldFailWhenNameIsEmpty()
    {
        _validator.Validate(new CreateBuyerCommand { Name = string.Empty }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailWhenNameExceedsLimit()
    {
        _validator.Validate(new CreateBuyerCommand { Name = new string('a', 101) }).Errors
            .ShouldContain(x => x.PropertyName == "Name");
    }

    [Test]
    public void ShouldPassWhenCommandIsValid()
    {
        _validator.Validate(new CreateBuyerCommand { Name = "Buyer" }).IsValid.ShouldBeTrue();
    }
}

public class UpdateBuyerCommandValidatorTests
{
    private readonly UpdateBuyerCommandValidator _validator = new();

    [Test]
    public void ShouldFailWhenNameIsEmpty()
    {
        _validator.Validate(new UpdateBuyerCommand { Id = 1, Name = string.Empty }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldFailWhenNameExceedsLimit()
    {
        _validator.Validate(new UpdateBuyerCommand { Id = 1, Name = new string('a', 101) }).Errors
            .ShouldContain(x => x.PropertyName == "Name");
    }

    [Test]
    public void ShouldPassWhenCommandIsValid()
    {
        _validator.Validate(new UpdateBuyerCommand { Id = 1, Name = "Buyer" }).IsValid.ShouldBeTrue();
    }
}
