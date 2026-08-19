using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Yellowtail.API.Middleware;
using Yellowtail.Services.Exceptions;

namespace Yellowtail.Tests.Unit.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _sut = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Theory]
    [InlineData(typeof(NotFoundException), StatusCodes.Status404NotFound, "Not Found")]
    [InlineData(typeof(ValidationFailedException), StatusCodes.Status400BadRequest, "Validation Failed")]
    public async Task TryHandleAsync_KnownExceptionTypes_WritesExpectedStatusAndTitle(
        Type exceptionType, int expectedStatus, string expectedTitle)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "boom")!;
        var httpContext = new DefaultHttpContext();
        var body = new MemoryStream();
        httpContext.Response.Body = body;

        var handled = await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatus, httpContext.Response.StatusCode);

        body.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(body);
        Assert.Equal(expectedTitle, problem!.Title);
        Assert.Equal("boom", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_Returns500WithGenericTitle()
    {
        var httpContext = new DefaultHttpContext();
        var body = new MemoryStream();
        httpContext.Response.Body = body;

        var handled = await _sut.TryHandleAsync(httpContext, new InvalidOperationException("oops"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

        body.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(body);
        Assert.Equal("An unexpected error occurred", problem!.Title);
    }
}
