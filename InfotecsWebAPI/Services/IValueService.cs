using InfotecsWebAPI.Models;

namespace InfotecsWebAPI.Services;

public interface IValueService
{
    /// <summary>
    /// Gets the last values from the database for a specific file.
    /// </summary>
    /// <param name="fileName">Filename for query</param>
    /// <param name="descending">Sort by descending</param>
    /// <param name="limit">Limit for values</param>
    /// <returns></returns>
    public Task<IEnumerable<ValueEntity>> GetLastValuesAsync(
        string fileName,
        bool descending = true,
        int limit = 10);
}