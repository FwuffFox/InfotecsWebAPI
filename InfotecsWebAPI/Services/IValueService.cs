using InfotecsWebAPI.Models;

namespace InfotecsWebAPI.Services;

public interface IValueService
{
    public Task<IEnumerable<ValueEntity>> GetLastValuesAsync(
        string fileName,
        bool descending = true,
        int limit = 10);
}