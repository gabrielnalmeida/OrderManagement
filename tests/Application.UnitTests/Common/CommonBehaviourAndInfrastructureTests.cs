using FluentValidation.Results;
using OrderManagement.Application.Common.Caching;
using OrderManagement.Application.UnitTests.Common;

namespace OrderManagement.Application.UnitTests.Common;

public class ValidationBehaviourTests
{
    [Test]
    public async Task ShouldCallNextWhenThereAreNoValidators()
    {
        var behaviour = new ValidationBehaviour<TestRequest, string>([]);
        var nextCalled = false;

        var response = await behaviour.Handle(new TestRequest(), _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }, CancellationToken.None);

        response.ShouldBe("ok");
        nextCalled.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldCallNextWhenValidationSucceeds()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var behaviour = new ValidationBehaviour<TestRequest, string>([validator.Object]);
        var nextCalled = false;

        var response = await behaviour.Handle(new TestRequest(), _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }, CancellationToken.None);

        response.ShouldBe("ok");
        nextCalled.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldThrowValidationExceptionWhenValidationFails()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult([new ValidationFailure("Name", "invalid")]));

        var behaviour = new ValidationBehaviour<TestRequest, string>([validator.Object]);
        var nextCalled = false;

        var action = async () => await behaviour.Handle(new TestRequest(), _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }, CancellationToken.None);

        var exception = await action.ShouldThrowAsync<OrderManagement.Application.Common.Exceptions.ValidationException>();
        exception.Errors.ShouldContainKey("Name");
        nextCalled.ShouldBeFalse();
    }
}

public class CachingBehaviourTests
{
    [Test]
    public async Task ShouldBypassCacheForNonCacheableRequests()
    {
        var cache = new Mock<IApplicationCache>();
        var behaviour = new CachingBehaviour<TestRequest, string>(cache.Object);

        var response = await behaviour.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        response.ShouldBe("ok");
        cache.Verify(x => x.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnCachedResponseWhenCacheHitOccurs()
    {
        var cache = new Mock<IApplicationCache>();
        cache.Setup(x => x.GetAsync<string>("cache-key", It.IsAny<CancellationToken>())).ReturnsAsync("cached");
        var behaviour = new CachingBehaviour<TestCacheableRequest, string>(cache.Object);
        var nextCalled = false;

        var response = await behaviour.Handle(new TestCacheableRequest(), _ =>
        {
            nextCalled = true;
            return Task.FromResult("fresh");
        }, CancellationToken.None);

        response.ShouldBe("cached");
        nextCalled.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldCallNextAndStoreResponseWhenCacheMissOccurs()
    {
        var cache = new Mock<IApplicationCache>();
        cache.Setup(x => x.GetAsync<string>("cache-key", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var behaviour = new CachingBehaviour<TestCacheableRequest, string>(cache.Object);

        var response = await behaviour.Handle(new TestCacheableRequest(), _ => Task.FromResult("fresh"), CancellationToken.None);

        response.ShouldBe("fresh");
        cache.Verify(x => x.SetAsync("cache-key", "fresh", TimeSpan.FromMinutes(1), It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class UnhandledExceptionBehaviourTests
{
    [Test]
    public async Task ShouldReturnResponseWhenNoExceptionOccurs()
    {
        var logger = new TestLogger<TestRequest>();
        var behaviour = new UnhandledExceptionBehaviour<TestRequest, string>(logger);

        var response = await behaviour.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        response.ShouldBe("ok");
        logger.Entries.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldLogAndRethrowWhenExceptionOccurs()
    {
        var logger = new TestLogger<TestRequest>();
        var behaviour = new UnhandledExceptionBehaviour<TestRequest, string>(logger);

        var action = async () => await behaviour.Handle(new TestRequest(), _ => throw new InvalidOperationException("boom"), CancellationToken.None);

        await action.ShouldThrowAsync<InvalidOperationException>();
        logger.Entries.ShouldContain(x => x.Level == LogLevel.Error && x.Exception is InvalidOperationException);
    }
}

public class LoggingBehaviourTests
{
    [Test]
    public async Task ShouldLogRequestInformation()
    {
        var logger = new TestLogger<TestRequest>();
        var behaviour = new LoggingBehaviour<TestRequest>(logger);

        await behaviour.Process(new TestRequest(), CancellationToken.None);

        logger.Entries.ShouldContain(x => x.Level == LogLevel.Information && x.Message.Contains("TestRequest", StringComparison.Ordinal));
    }
}

public class PerformanceBehaviourTests
{
    [Test]
    public async Task ShouldReturnResponseFromNext()
    {
        var logger = new TestLogger<TestRequest>();
        var behaviour = new PerformanceBehaviour<TestRequest, string>(logger);

        var response = await behaviour.Handle(new TestRequest(), _ => Task.FromResult("ok"), CancellationToken.None);

        response.ShouldBe("ok");
    }

    [Test]
    public async Task ShouldLogWarningWhenRequestIsSlow()
    {
        var logger = new TestLogger<TestRequest>();
        var behaviour = new PerformanceBehaviour<TestRequest, string>(logger);

        await behaviour.Handle(new TestRequest(), async _ =>
        {
            await Task.Delay(550);
            return "ok";
        }, CancellationToken.None);

        logger.Entries.ShouldContain(x => x.Level == LogLevel.Warning && x.Message.Contains("TestRequest", StringComparison.Ordinal));
    }
}

public class ValidationExceptionTests
{
    [Test]
    public void ShouldCreateEmptyErrorsDictionaryWhenUsingDefaultConstructor()
    {
        var exception = new OrderManagement.Application.Common.Exceptions.ValidationException();

        exception.Errors.ShouldBeEmpty();
    }

    [Test]
    public void ShouldGroupErrorsByPropertyName()
    {
        var exception = new OrderManagement.Application.Common.Exceptions.ValidationException(
        [
            new ValidationFailure("Name", "required"),
            new ValidationFailure("Name", "too long"),
            new ValidationFailure("Price", "invalid")
        ]);

        exception.Errors["Name"].ShouldBe(["required", "too long"]);
        exception.Errors["Price"].ShouldBe(["invalid"]);
    }
}

public class CachingKeysTests
{
    [Test]
    public void ShouldGenerateStableOrdersListCacheKey()
    {
        var key = CachingKeys.GetOrdersListCacheKey(1, "Started", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc));

        key.ShouldBe("orders:list:buyer:1:status:Started:from:2026-04-01T00:00:00.0000000Z:to:2026-04-02T00:00:00.0000000Z");
    }

    [Test]
    public void ShouldUseNullTokensWhenFiltersAreMissing()
    {
        var key = CachingKeys.GetOrdersListCacheKey(null, null, null, null);

        key.ShouldBe("orders:list:buyer:null:status:null:from:null:to:null");
    }
}

public class ApplicationMemoryCacheTests
{
    [Test]
    public async Task ShouldStoreAndRetrieveValue()
    {
        var cache = new ApplicationMemoryCache(new MemoryCache(new MemoryCacheOptions()));

        await cache.SetAsync("a", "value", TimeSpan.FromMinutes(1), CancellationToken.None);

        (await cache.GetAsync<string>("a", CancellationToken.None)).ShouldBe("value");
    }

    [Test]
    public async Task ShouldReturnNullWhenKeyDoesNotExist()
    {
        var cache = new ApplicationMemoryCache(new MemoryCache(new MemoryCacheOptions()));

        (await cache.GetAsync<string>("missing", CancellationToken.None)).ShouldBeNull();
    }

    [Test]
    public async Task ShouldRemoveAllKeysByPrefix()
    {
        var cache = new ApplicationMemoryCache(new MemoryCache(new MemoryCacheOptions()));

        await cache.SetAsync("orders:list:1", "a", TimeSpan.FromMinutes(1), CancellationToken.None);
        await cache.SetAsync("orders:list:2", "b", TimeSpan.FromMinutes(1), CancellationToken.None);
        await cache.SetAsync("products:list:1", "c", TimeSpan.FromMinutes(1), CancellationToken.None);

        await cache.RemoveByPrefixAsync("orders:list:", CancellationToken.None);

        (await cache.GetAsync<string>("orders:list:1", CancellationToken.None)).ShouldBeNull();
        (await cache.GetAsync<string>("orders:list:2", CancellationToken.None)).ShouldBeNull();
        (await cache.GetAsync<string>("products:list:1", CancellationToken.None)).ShouldBe("c");
    }

    [Test]
    public async Task ShouldNotRemoveKeysWithDifferentPrefix()
    {
        var cache = new ApplicationMemoryCache(new MemoryCache(new MemoryCacheOptions()));

        await cache.SetAsync("buyers:list:1", "a", TimeSpan.FromMinutes(1), CancellationToken.None);

        await cache.RemoveByPrefixAsync("orders:list:", CancellationToken.None);

        (await cache.GetAsync<string>("buyers:list:1", CancellationToken.None)).ShouldBe("a");
    }
}

public class ProblemDetailsExceptionHandlerTests
{
    [Test]
    public async Task ShouldHandleValidationExceptionAsValidationProblemDetails()
    {
        var handler = new ProblemDetailsExceptionHandler();
        var context = CreateHttpContext();
        var exception = new OrderManagement.Application.Common.Exceptions.ValidationException([new ValidationFailure("Name", "required")]);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        var body = await ReadResponseBodyAsync(context);

        handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("validation errors");
    }

    [Test]
    public async Task ShouldHandleNotFoundExceptionAsProblemDetails()
    {
        var handler = new ProblemDetailsExceptionHandler();
        var context = CreateHttpContext();

        var handled = await handler.TryHandleAsync(context, new NotFoundException("Order", "1"), CancellationToken.None);

        handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        (await ReadResponseBodyAsync(context)).ShouldContain("Order");
    }

    [Test]
    public async Task ShouldReturnFalseForUnknownException()
    {
        var handler = new ProblemDetailsExceptionHandler();
        var context = CreateHttpContext();

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        handled.ShouldBeFalse();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadResponseBodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}

public record TestRequest;

public record TestCacheableRequest : ICacheableQuery
{
    public string CacheKey => "cache-key";

    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}
