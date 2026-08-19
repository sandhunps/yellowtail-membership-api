using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Yellowtail.API.Contracts;
using Yellowtail.API.Filters;
using Yellowtail.API.Validators;

namespace Yellowtail.Tests.Unit.Filters;

public class ValidationFilterTests
{
    private static ActionExecutingContext CreateContext(IServiceProvider services, IDictionary<string, object?> arguments)
    {
        var httpContext = new DefaultHttpContext { RequestServices = services };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            arguments,
            controller: new object());
    }

    private static Task<ActionExecutedContext> NoOpNext(ActionExecutingContext context) =>
        Task.FromResult(new ActionExecutedContext(context, context.Filters, context.Controller));

    [Fact]
    public async Task OnActionExecutionAsync_InvalidArgumentWithRegisteredValidator_ShortCircuitsWith400()
    {
        var services = new ServiceCollection()
            .AddSingleton<IValidator<CreateMemberRequest>, CreateMemberRequestValidator>()
            .BuildServiceProvider();

        var invalidRequest = new CreateMemberRequest { FirstName = "", LastName = "Lovelace", Email = "not-an-email" };
        var context = CreateContext(services, new Dictionary<string, object?> { ["request"] = invalidRequest });

        var nextCalled = false;
        Task<ActionExecutedContext> Next()
        {
            nextCalled = true;
            return NoOpNext(context);
        }

        var sut = new ValidationFilter();
        await sut.OnActionExecutionAsync(context, Next);

        Assert.False(nextCalled);
        var badRequest = Assert.IsType<BadRequestObjectResult>(context.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.True(problem.Errors.ContainsKey("FirstName"));
        Assert.True(problem.Errors.ContainsKey("Email"));
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidArgument_InvokesNext()
    {
        var services = new ServiceCollection()
            .AddSingleton<IValidator<CreateMemberRequest>, CreateMemberRequestValidator>()
            .BuildServiceProvider();

        var validRequest = new CreateMemberRequest { FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" };
        var context = CreateContext(services, new Dictionary<string, object?> { ["request"] = validRequest });

        var nextCalled = false;
        Task<ActionExecutedContext> Next()
        {
            nextCalled = true;
            return NoOpNext(context);
        }

        var sut = new ValidationFilter();
        await sut.OnActionExecutionAsync(context, Next);

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ArgumentWithNoRegisteredValidator_InvokesNextWithoutValidating()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var context = CreateContext(services, new Dictionary<string, object?> { ["id"] = Guid.NewGuid() });

        var nextCalled = false;
        Task<ActionExecutedContext> Next()
        {
            nextCalled = true;
            return NoOpNext(context);
        }

        var sut = new ValidationFilter();
        await sut.OnActionExecutionAsync(context, Next);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task OnActionExecutionAsync_NullArgument_SkipsAndInvokesNext()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var context = CreateContext(services, new Dictionary<string, object?> { ["request"] = null });

        var nextCalled = false;
        Task<ActionExecutedContext> Next()
        {
            nextCalled = true;
            return NoOpNext(context);
        }

        var sut = new ValidationFilter();
        await sut.OnActionExecutionAsync(context, Next);

        Assert.True(nextCalled);
    }
}
