using InfotecsWebAPI.Data;
using InfotecsWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InfotecsWebAPI.Services;

public class ValueService(TimescaleDbContext dbContext) : IValueService
{
    public async Task<IEnumerable<ValueEntity>> GetLastValuesAsync(string fileName, bool descending = true,
        int limit = 10)
    {
        var query = dbContext.Values.AsQueryable()
            .Where(v => v.FileName == fileName);

        query = descending ? query.OrderByDescending(v => v.Date) : query.OrderBy(v => v.Date);

        query = query.Take(limit);

        return await query.ToListAsync();
    }
}