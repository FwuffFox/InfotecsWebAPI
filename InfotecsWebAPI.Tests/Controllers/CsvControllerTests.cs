using System.Diagnostics;
using System.Text;
using FluentAssertions;
using InfotecsWebAPI.Controllers;
using InfotecsWebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfotecsWebAPI.Tests.Controllers;

/// <summary>
///     Unit tests for CsvController functionality.
/// </summary>
public class CsvControllerTests : IDisposable
{
    private readonly ActivitySource _activitySource;
    private readonly CsvController _controller;
    private readonly Mock<ICsvProcessingService> _csvProcessingServiceMock;

    public CsvControllerTests()
    {
        _csvProcessingServiceMock = new Mock<ICsvProcessingService>();
        var loggerMock = new Mock<ILogger<CsvController>>();
        _activitySource = new ActivitySource("TestActivitySource");

        _controller = new CsvController(
            _csvProcessingServiceMock.Object,
            loggerMock.Object,
            _activitySource
        );
    }

    public void Dispose()
    {
        _activitySource?.Dispose();
    }

    [Fact]
    public async Task UploadCsv_WithValidFile_ReturnsOkResult()
    {
        // Arrange
        var csvContent = "Date,Value,Type\n2023-01-01,100,A";
        var mockFile = CreateMockFormFile("test.csv", csvContent, "text/csv");

        _csvProcessingServiceMock
            .Setup(x => x.ProcessCsvFileAsync(It.IsAny<Stream>(), "test.csv"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UploadCsv(mockFile);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        _csvProcessingServiceMock.Verify(
            x => x.ProcessCsvFileAsync(It.IsAny<Stream>(), "test.csv"),
            Times.Once
        );
    }

    [Fact]
    public async Task UploadCsv_WithNullFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UploadCsv(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadCsv_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var mockFile = CreateMockFormFile("empty.csv", "", "text/csv");

        // Act
        var result = await _controller.UploadCsv(mockFile);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadCsv_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.txt", "some content", "text/plain");

        // Act
        var result = await _controller.UploadCsv(mockFile);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadCsv_WithTooLargeFile_ReturnsBadRequest()
    {
        // Arrange
        var largeContent = new string('a', 51 * 1024 * 1024); // 51 MB
        var mockFile = CreateMockFormFile("large.csv", largeContent, "text/csv");

        // Act
        var result = await _controller.UploadCsv(mockFile);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadCsv_WhenProcessingThrowsValidationException_ReturnsBadRequest()
    {
        // Arrange
        var csvContent = "Date,Value,Type\n2023-01-01,invalid,A";
        var mockFile = CreateMockFormFile("test.csv", csvContent, "text/csv");

        _csvProcessingServiceMock
            .Setup(x => x.ProcessCsvFileAsync(It.IsAny<Stream>(), "test.csv"))
            .ThrowsAsync(new FormatException("Invalid format"));

        // Act
        var result = await _controller.UploadCsv(mockFile);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadCsv_WhenProcessingThrowsUnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        var csvContent = "Date,Value,Type\n2023-01-01,100,A";
        var mockFile = CreateMockFormFile("test.csv", csvContent, "text/csv");

        _csvProcessingServiceMock
            .Setup(x => x.ProcessCsvFileAsync(It.IsAny<Stream>(), "test.csv"))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.UploadCsv(mockFile);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    private static IFormFile CreateMockFormFile(string fileName, string content, string contentType)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(bytes.Length);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        return mockFile.Object;
    }
}