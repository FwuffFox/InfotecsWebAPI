using InfotecsWebAPI.Data;
using InfotecsWebAPI.Models;
using InfotecsWebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace InfotecsWebAPI.Tests.Services;

/// <summary>
/// Unit tests for ValueService functionality.
/// </summary>
public class ValueServiceTests : IDisposable
{
    private readonly TimescaleDbContext _context;
    private readonly IValueService _valueService;
    private readonly ServiceProvider _serviceProvider;

    public ValueServiceTests()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<TimescaleDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddScoped<IValueService, ValueService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TimescaleDbContext>();
        _valueService = _serviceProvider.GetRequiredService<IValueService>();

        SeedTestData();
    }

    private void SeedTestData()
    {
        var testValues = new List<ValueEntity>
        {
            // File 1: test1.csv - 5 entries with different dates
            new()
            {
                Id = 1,
                FileName = "test1.csv",
                Date = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
                ExecutionTime = 1.5m,
                Value = 10.5m
            },
            new()
            {
                Id = 2,
                FileName = "test1.csv",
                Date = new DateTimeOffset(2024, 1, 2, 11, 0, 0, TimeSpan.Zero),
                ExecutionTime = 2.0m,
                Value = 15.2m
            },
            new()
            {
                Id = 3,
                FileName = "test1.csv",
                Date = new DateTimeOffset(2024, 1, 3, 12, 0, 0, TimeSpan.Zero),
                ExecutionTime = 1.8m,
                Value = 20.7m
            },
            new()
            {
                Id = 4,
                FileName = "test1.csv",
                Date = new DateTimeOffset(2024, 1, 4, 13, 0, 0, TimeSpan.Zero),
                ExecutionTime = 2.2m,
                Value = 25.1m
            },
            new()
            {
                Id = 5,
                FileName = "test1.csv",
                Date = new DateTimeOffset(2024, 1, 5, 14, 0, 0, TimeSpan.Zero),
                ExecutionTime = 1.9m,
                Value = 30.3m
            },
            
            // File 2: test2.csv - 3 entries
            new()
            {
                Id = 6,
                FileName = "test2.csv",
                Date = new DateTimeOffset(2024, 1, 1, 15, 0, 0, TimeSpan.Zero),
                ExecutionTime = 3.0m,
                Value = 50.0m
            },
            new()
            {
                Id = 7,
                FileName = "test2.csv",
                Date = new DateTimeOffset(2024, 1, 2, 16, 0, 0, TimeSpan.Zero),
                ExecutionTime = 2.8m,
                Value = 45.5m
            },
            new()
            {
                Id = 8,
                FileName = "test2.csv",
                Date = new DateTimeOffset(2024, 1, 3, 17, 0, 0, TimeSpan.Zero),
                ExecutionTime = 3.2m,
                Value = 55.8m
            },
            
            // File 3: data.csv - 2 entries
            new()
            {
                Id = 9,
                FileName = "data.csv",
                Date = new DateTimeOffset(2024, 1, 1, 18, 0, 0, TimeSpan.Zero),
                ExecutionTime = 4.0m,
                Value = 100.0m
            },
            new()
            {
                Id = 10,
                FileName = "data.csv",
                Date = new DateTimeOffset(2024, 1, 2, 19, 0, 0, TimeSpan.Zero),
                ExecutionTime = 3.8m,
                Value = 95.5m
            }
        };

        _context.Values.AddRange(testValues);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetLastValuesAsync_WithValidFileName_ShouldReturnValuesForThatFile()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv")).ToList();

        // Assert
        results.Should().HaveCount(5);
        results.Should().OnlyContain(v => v.FileName == "test1.csv");
    }

    [Fact]
    public async Task GetLastValuesAsync_WithDescendingOrder_ShouldReturnLatestFirst()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", descending: true)).ToList();

        // Assert
        results.Should().HaveCount(5);
        results.Should().BeInDescendingOrder(v => v.Date);
        results.First().Date.Should().Be(new DateTimeOffset(2024, 1, 5, 14, 0, 0, TimeSpan.Zero));
        results.Last().Date.Should().Be(new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task GetLastValuesAsync_WithAscendingOrder_ShouldReturnOldestFirst()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", descending: false)).ToList();

        // Assert
        results.Should().HaveCount(5);
        results.Should().BeInAscendingOrder(v => v.Date);
        results.First().Date.Should().Be(new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
        results.Last().Date.Should().Be(new DateTimeOffset(2024, 1, 5, 14, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task GetLastValuesAsync_WithDefaultParameters_ShouldUseDescendingOrderAndLimit10()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv")).ToList();

        // Assert
        results.Should().HaveCount(5); // Only 5 entries for test1.csv, less than default limit of 10
        results.Should().BeInDescendingOrder(v => v.Date);
    }

    [Fact]
    public async Task GetLastValuesAsync_WithLimit_ShouldRespectLimit()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", limit: 3)).ToList();

        // Assert
        results.Should().HaveCount(3);
        results.Should().BeInDescendingOrder(v => v.Date);
        // Should get the 3 most recent entries
        results.Select(v => v.Value).Should().ContainInOrder(30.3m, 25.1m, 20.7m);
    }

    [Fact]
    public async Task GetLastValuesAsync_WithLimitLargerThanAvailable_ShouldReturnAllAvailable()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test2.csv", limit: 10)).ToList();

        // Assert
        results.Should().HaveCount(3); // Only 3 entries available for test2.csv
        results.Should().OnlyContain(v => v.FileName == "test2.csv");
        results.Should().BeInDescendingOrder(v => v.Date);
    }

    [Fact]
    public async Task GetLastValuesAsync_WithLimit1_ShouldReturnMostRecentOnly()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", limit: 1)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results.First().Date.Should().Be(new DateTimeOffset(2024, 1, 5, 14, 0, 0, TimeSpan.Zero));
        results.First().Value.Should().Be(30.3m);
    }

    [Fact]
    public async Task GetLastValuesAsync_WithAscendingAndLimit_ShouldReturnOldestEntries()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", descending: false, limit: 2)).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().BeInAscendingOrder(v => v.Date);
        results.First().Date.Should().Be(new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
        results.Last().Date.Should().Be(new DateTimeOffset(2024, 1, 2, 11, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task GetLastValuesAsync_WithNonexistentFileName_ShouldReturnEmptyCollection()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("nonexistent.csv")).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLastValuesAsync_WithEmptyFileName_ShouldReturnEmptyCollection()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("")).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLastValuesAsync_WithZeroLimit_ShouldReturnEmptyCollection()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", limit: 0)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLastValuesAsync_WithNegativeLimit_ShouldReturnEmptyCollection()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", limit: -1)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLastValuesAsync_WithDifferentFiles_ShouldReturnCorrectFileData()
    {
        // Act
        var test1Results = (await _valueService.GetLastValuesAsync("test1.csv")).ToList();
        var test2Results = (await _valueService.GetLastValuesAsync("test2.csv")).ToList();
        var dataResults = (await _valueService.GetLastValuesAsync("data.csv")).ToList();

        // Assert
        test1Results.Should().HaveCount(5);
        test1Results.Should().OnlyContain(v => v.FileName == "test1.csv");

        test2Results.Should().HaveCount(3);
        test2Results.Should().OnlyContain(v => v.FileName == "test2.csv");

        dataResults.Should().HaveCount(2);
        dataResults.Should().OnlyContain(v => v.FileName == "data.csv");

        // Verify no cross-contamination between files
        var allFileNames = test1Results.Concat(test2Results).Concat(dataResults)
            .Select(v => v.FileName).Distinct();
        allFileNames.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetLastValuesAsync_ShouldReturnCompleteValueEntity()
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync("test1.csv", limit: 1)).ToList();

        // Assert
        results.Should().HaveCount(1);
        var result = results.First();
        
        result.Id.Should().BePositive();
        result.FileName.Should().Be("test1.csv");
        result.Date.Should().NotBe(default);
        result.ExecutionTime.Should().BePositive();
        result.Value.Should().BePositive();
    }

    [Theory]
    [InlineData("test1.csv", 5)]
    [InlineData("test2.csv", 3)]
    [InlineData("data.csv", 2)]
    [InlineData("missing.csv", 0)]
    public async Task GetLastValuesAsync_WithVariousFileNames_ShouldReturnCorrectCounts(string fileName, int expectedCount)
    {
        // Act
        var results = (await _valueService.GetLastValuesAsync(fileName)).ToList();

        // Assert
        results.Should().HaveCount(expectedCount);
        if (expectedCount > 0)
        {
            results.Should().OnlyContain(v => v.FileName == fileName);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
