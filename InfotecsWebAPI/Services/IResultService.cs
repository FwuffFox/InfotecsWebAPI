using InfotecsWebAPI.Models;

namespace InfotecsWebAPI.Services;

/// <summary>
///     Interface for result data operations and filtering.
/// </summary>
public interface IResultService
{
    /// <summary>
    ///     Gets filtered results from the database based on provided criteria.
    /// </summary>
    /// <param name="fileName">Filter by file name (optional)</param>
    /// <param name="minStartTime">Minimum start time for filtering (optional)</param>
    /// <param name="maxStartTime">Maximum start time for filtering (optional)</param>
    /// <param name="minAvgValue">Minimum average value for filtering (optional)</param>
    /// <param name="maxAvgValue">Maximum average value for filtering (optional)</param>
    /// <param name="minAvgExecutionTime">Minimum average execution time for filtering (optional)</param>
    /// <param name="maxAvgExecutionTime">Maximum average execution time for filtering (optional)</param>
    /// <returns>Filtered collection of result entities</returns>
    Task<IEnumerable<ResultEntity>> GetFilteredResultsAsync(
        string? fileName = null,
        DateTimeOffset? minStartTime = null,
        DateTimeOffset? maxStartTime = null,
        decimal? minAvgValue = null,
        decimal? maxAvgValue = null,
        decimal? minAvgExecutionTime = null,
        decimal? maxAvgExecutionTime = null);
}