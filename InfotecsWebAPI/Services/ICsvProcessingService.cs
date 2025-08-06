namespace InfotecsWebAPI.Services;

/// <summary>
///     Service interface for processing CSV files.
/// </summary>
public interface ICsvProcessingService
{
    /// <summary>
    ///     Processes CSV file and saves data to database.
    /// </summary>
    /// <param name="csvContent">CSV file content as stream</param>
    /// <param name="fileName">Name of the CSV file</param>
    /// <returns>Task representing the async operation</returns>
    Task ProcessCsvFileAsync(Stream csvContent, string fileName);
}