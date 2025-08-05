using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using InfotecsWebAPI.Data;
using InfotecsWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InfotecsWebAPI.Services;

/// <summary>
/// Service for processing CSV files with timescale data.
/// </summary>
internal class CsvProcessingService(TimescaleDbContext dbContext) : ICsvProcessingService
{
    /// <summary>
    /// Processes CSV file and saves data to database with validation.
    /// </summary>
    public async Task ProcessCsvFileAsync(Stream csvContent, string fileName)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var values =
                ParseAndValidateCsvAsync(csvContent, fileName)
                    .ToBlockingEnumerable()
                    .ToList();
            
            await RemoveExistingDataAsync(fileName);

            await dbContext.Values.AddRangeAsync(values);
            await dbContext.SaveChangesAsync();
            
            var result = CalculateResults(values, fileName);
            await dbContext.Results.AddAsync(result);
            await dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Parses CSV content
    /// </summary>
    private IAsyncEnumerable<ValueEntity> ParseAndValidateCsvAsync(Stream csvContent, string fileName)
    {
        using var reader = new StreamReader(csvContent);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });
        return csv.GetRecordsAsync<ValueEntity>();
    }

    /// <summary>
    /// Removes existing data for the specified file.
    /// </summary>
    private async Task RemoveExistingDataAsync(string fileName)
    {
        var existingResults = await dbContext.Results
            .Where(r => r.FileName == fileName)
            .ToListAsync();
        dbContext.Results.RemoveRange(existingResults);
        
        var existingValues = await dbContext.Values
            .Where(v => v.FileName == fileName)
            .ToListAsync();
        dbContext.Values.RemoveRange(existingValues);

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Calculates aggregated results from the values.
    /// </summary>
    private static ResultEntity CalculateResults(List<ValueEntity> values, string fileName)
    {
        var dates = values.Select(v => v.Date).OrderBy(d => d).ToList();
        var executionTimes = values.Select(v => v.ExecutionTime).ToList();
        var valuesList = values.Select(v => v.Value).OrderBy(v => v).ToList();
        
        var timeDelta = (long)(dates.Last() - dates.First()).TotalSeconds;
        
        var median = valuesList.Count % 2 == 0
            ? (valuesList[valuesList.Count / 2 - 1] + valuesList[valuesList.Count / 2]) / 2
            : valuesList[valuesList.Count / 2];

        return new ResultEntity
        {
            FileName = fileName,
            TimeDelta = timeDelta,
            MinStartTime = dates.First(),
            AvgExecutionTime = executionTimes.Average(),
            AvgValue = valuesList.Average(),
            MedianValue = median,
            MaxValue = valuesList.Max(),
            MinValue = valuesList.Min()
        };
    }
}