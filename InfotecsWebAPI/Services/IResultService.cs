using InfotecsWebAPI.Models;

namespace InfotecsWebAPI.Services;

/// <summary>
/// Interface for result data operations and filtering.
/// </summary>
public interface IResultService
{
    /// <summary>
    /// Gets filtered results from the database based on provided criteria.
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

    /// <summary>
    /// Gets a specific result by its ID.
    /// </summary>
    /// <param name="id">The result ID</param>
    /// <returns>The result entity if found, null otherwise</returns>
    Task<ResultEntity?> GetResultByIdAsync(int id);

    /// <summary>
    /// Gets all results for a specific file name.
    /// </summary>
    /// <param name="fileName">The file name to filter by</param>
    /// <returns>Collection of result entities for the specified file</returns>
    Task<IEnumerable<ResultEntity>> GetResultsByFileNameAsync(string fileName);

    /// <summary>
    /// Gets results within a specific time range.
    /// </summary>
    /// <param name="startTime">Start of the time range</param>
    /// <param name="endTime">End of the time range</param>
    /// <returns>Collection of result entities within the time range</returns>
    Task<IEnumerable<ResultEntity>> GetResultsByTimeRangeAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// Gets the count of results matching the filter criteria.
    /// </summary>
    /// <param name="fileName">Filter by file name (optional)</param>
    /// <param name="minStartTime">Minimum start time for filtering (optional)</param>
    /// <param name="maxStartTime">Maximum start time for filtering (optional)</param>
    /// <param name="minAvgValue">Minimum average value for filtering (optional)</param>
    /// <param name="maxAvgValue">Maximum average value for filtering (optional)</param>
    /// <param name="minAvgExecutionTime">Minimum average execution time for filtering (optional)</param>
    /// <param name="maxAvgExecutionTime">Maximum average execution time for filtering (optional)</param>
    /// <returns>Count of matching results</returns>
    Task<int> GetFilteredResultsCountAsync(
        string? fileName = null,
        DateTime? minStartTime = null,
        DateTime? maxStartTime = null,
        decimal? minAvgValue = null,
        decimal? maxAvgValue = null,
        decimal? minAvgExecutionTime = null,
        decimal? maxAvgExecutionTime = null);
}