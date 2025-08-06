using InfotecsWebAPI.Data;
using InfotecsWebAPI.Models;
using InfotecsWebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace InfotecsWebAPI.Tests.Services;

/// <summary>
/// Unit tests for ResultService functionality.
/// </summary>
public class ResultServiceTests : IDisposable
{
    private readonly TimescaleDbContext _context;
    private readonly IResultService _resultService;
    private readonly ServiceProvider _serviceProvider;

    public ResultServiceTests()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<TimescaleDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddScoped<IResultService, ResultService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TimescaleDbContext>();
        _resultService = _serviceProvider.GetRequiredService<IResultService>();

        SeedTestData();
    }

    private void SeedTestData()
    {
        var testResults = new List<ResultEntity>
        {
            new()
            {
                Id = 1,
                FileName = "test1.csv",
                TimeDelta = 100,
                MinStartTime = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
                AvgExecutionTime = 1.5m,
                AvgValue = 25.5m,
                MedianValue = 25.0m
            },
            new()
            {
                Id = 2,
                FileName = "test2.csv",
                TimeDelta = 200,
                MinStartTime = new DateTimeOffset(2024, 1, 2, 11, 0, 0, TimeSpan.Zero),
                AvgExecutionTime = 2.5m,
                AvgValue = 35.7m,
                MedianValue = 35.0m
            },
            new()
            {
                Id = 3,
                FileName = "data.csv",
                TimeDelta = 150,
                MinStartTime = new DateTimeOffset(2024, 1, 3, 12, 0, 0, TimeSpan.Zero),
                AvgExecutionTime = 1.8m,
                AvgValue = 45.2m,
                MedianValue = 44.5m
            },
            new()
            {
                Id = 4,
                FileName = "sample.csv",
                TimeDelta = 300,
                MinStartTime = new DateTimeOffset(2024, 1, 4, 13, 0, 0, TimeSpan.Zero),
                AvgExecutionTime = 3.2m,
                AvgValue = 15.8m,
                MedianValue = 16.0m
            }
        };

        _context.Results.AddRange(testResults);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithNoFilters_ShouldReturnAllResults()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync()).ToList();

        // Assert
        results.Should().HaveCount(4);
        results.Should().Contain(r => r.FileName == "test1.csv");
        results.Should().Contain(r => r.FileName == "test2.csv");
        results.Should().Contain(r => r.FileName == "data.csv");
        results.Should().Contain(r => r.FileName == "sample.csv");
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithExactFileNameFilter_ShouldReturnExactMatch()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(fileName: "data.csv")).ToList();

        // Assert
        results.Should().HaveCount(1);
        results.First().FileName.Should().Be("data.csv");
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithMinStartTimeFilter_ShouldReturnResultsAfterDate()
    {
        // Arrange
        var minStartTime = new DateTimeOffset(2024, 1, 2, 10, 30, 0, TimeSpan.Zero);

        // Act
        var results = (await _resultService.GetFilteredResultsAsync(minStartTime: minStartTime)).ToList();

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.MinStartTime >= minStartTime);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithMaxStartTimeFilter_ShouldReturnResultsBeforeDate()
    {
        // Arrange
        var maxStartTime = new DateTimeOffset(2024, 1, 2, 15, 0, 0, TimeSpan.Zero);

        // Act
        var results = (await _resultService.GetFilteredResultsAsync(maxStartTime: maxStartTime)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.MinStartTime <= maxStartTime);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithStartTimeRange_ShouldReturnResultsInRange()
    {
        // Arrange
        var minStartTime = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var maxStartTime = new DateTimeOffset(2024, 1, 3, 23, 59, 59, TimeSpan.Zero);

        // Act
        var results = (await _resultService.GetFilteredResultsAsync(
            minStartTime: minStartTime, 
            maxStartTime: maxStartTime)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.MinStartTime >= minStartTime && r.MinStartTime <= maxStartTime);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithMinAvgValueFilter_ShouldReturnResultsAboveValue()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(minAvgValue: 30.0m)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.AvgValue >= 30.0m);
        results.Select(r => r.FileName).Should().Contain("test2.csv", "data.csv");
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithMaxAvgValueFilter_ShouldReturnResultsBelowValue()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(maxAvgValue: 30.0m)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.AvgValue <= 30.0m);
        results.Select(r => r.FileName).Should().Contain("test1.csv", "sample.csv");
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithAvgValueRange_ShouldReturnResultsInRange()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(
            minAvgValue: 20.0m, 
            maxAvgValue: 40.0m)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.AvgValue >= 20.0m && r.AvgValue <= 40.0m);
        results.Select(r => r.FileName).Should().Contain("test1.csv", "test2.csv");
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithMinAvgExecutionTimeFilter_ShouldReturnResultsAboveTime()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(minAvgExecutionTime: 2.0m)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.AvgExecutionTime >= 2.0m);
        results.Select(r => r.FileName).Should().Contain("test2.csv", "sample.csv");
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithMaxAvgExecutionTimeFilter_ShouldReturnResultsBelowTime()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(maxAvgExecutionTime: 2.0m)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.AvgExecutionTime <= 2.0m);
        results.Select(r => r.FileName).Should().Contain("test1.csv", "data.csv");
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithAvgExecutionTimeRange_ShouldReturnResultsInRange()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(
            minAvgExecutionTime: 1.5m, 
            maxAvgExecutionTime: 2.5m)).ToList();

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.AvgExecutionTime >= 1.5m && r.AvgExecutionTime <= 2.5m);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(
            fileName: "test2.csv",
            minAvgValue: 30.0m,
            maxAvgExecutionTime: 3.0m)).ToList();

        // Assert
        results.Should().HaveCount(1);
        var result = results.First();
        result.FileName.Should().Be("test2.csv");
        result.AvgValue.Should().BeGreaterOrEqualTo(30.0m);
        result.AvgExecutionTime.Should().BeLessOrEqualTo(3.0m);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithNoMatchingFilters_ShouldReturnEmptyCollection()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(
            fileName: "nonexistent",
            minAvgValue: 1000.0m)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithNullFileName_ShouldIgnoreFileNameFilter()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(fileName: null)).ToList();

        // Assert
        results.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithEmptyFileName_ShouldIgnoreFileNameFilter()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(fileName: "")).ToList();

        // Assert
        results.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithWhitespaceFileName_ShouldIgnoreFileNameFilter()
    {
        // Act
        var results = (await _resultService.GetFilteredResultsAsync(fileName: "   ")).ToList();

        // Assert
        results.Should().HaveCount(4);
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
