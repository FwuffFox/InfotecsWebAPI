using InfotecsWebAPI.Data;
using InfotecsWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InfotecsWebAPI.Services;

public sealed class ResultService(TimescaleDbContext dbContext) : IResultService
{
    public async Task<IEnumerable<ResultEntity>> GetFilteredResultsAsync(string? fileName = null,
        DateTimeOffset? minStartTime = null, DateTimeOffset? maxStartTime = null,
        decimal? minAvgValue = null, decimal? maxAvgValue = null, decimal? minAvgExecutionTime = null,
        decimal? maxAvgExecutionTime = null)
    {
        var query = dbContext.Results.AsQueryable();
        if (!string.IsNullOrWhiteSpace(fileName))
            query = query.Where(r => r.FileName == fileName);
        if (minStartTime.HasValue)
            query = query.Where(r => r.MinStartTime >= minStartTime.Value);
        if (maxStartTime.HasValue)
            query = query.Where(r => r.MinStartTime <= maxStartTime.Value);
        if (minAvgValue.HasValue)
            query = query.Where(r => r.AvgValue >= minAvgValue.Value);
        if (maxAvgValue.HasValue)
            query = query.Where(r => r.AvgValue <= maxAvgValue.Value);
        if (minAvgExecutionTime.HasValue)
            query = query.Where(r => r.AvgExecutionTime >= minAvgExecutionTime.Value);
        if (maxAvgExecutionTime.HasValue)
            query = query.Where(r => r.AvgExecutionTime <= maxAvgExecutionTime.Value);

        return await query.ToListAsync();
    }
}