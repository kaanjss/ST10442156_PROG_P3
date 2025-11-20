using CMCS.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CMCS.Tests.Middleware;

public class ErrorHandlingTests
{
	[Fact]
	public async Task ErrorHandlingMiddleware_CatchesException_ReturnsErrorMessage()
	{
		// Arrange
		var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();

		RequestDelegate next = (HttpContext ctx) => throw new Exception("Test error message");

		var middleware = new ErrorHandlingMiddleware(next, loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		Assert.Equal(500, context.Response.StatusCode);
		Assert.Equal("application/json", context.Response.ContentType);

		context.Response.Body.Seek(0, SeekOrigin.Begin);
		var reader = new StreamReader(context.Response.Body);
		var responseBody = await reader.ReadToEndAsync();
		
		Assert.Contains("Test error message", responseBody);
	}

	[Fact]
	public async Task ErrorHandlingMiddleware_NoException_ContinuesPipeline()
	{
		// Arrange
		var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
		var context = new DefaultHttpContext();
		var nextCalled = false;

		RequestDelegate next = (HttpContext ctx) =>
		{
			nextCalled = true;
			return Task.CompletedTask;
		};

		var middleware = new ErrorHandlingMiddleware(next, loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		Assert.True(nextCalled);
	}

	[Fact]
	public async Task ErrorHandlingMiddleware_LogsException()
	{
		// Arrange
		var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();

		var testException = new Exception("Test exception");
		RequestDelegate next = (HttpContext ctx) => throw testException;

		var middleware = new ErrorHandlingMiddleware(next, loggerMock.Object);

		// Act
		await middleware.InvokeAsync(context);

		// Assert
		loggerMock.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => true),
				testException,
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}
}

