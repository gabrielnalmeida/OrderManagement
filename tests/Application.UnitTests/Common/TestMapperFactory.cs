namespace OrderManagement.Application.UnitTests.Common;

public static class TestMapperFactory
{
    private static readonly Lazy<IMapper> Mapper = new(CreateMapper);

    public static IMapper Instance => Mapper.Value;

    private static IMapper CreateMapper()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddMaps(typeof(CreateOrderCommand).Assembly);

        var configuration = new MapperConfiguration(expression, new LoggerFactory());
        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }
}
