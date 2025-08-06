using Microsoft.AspNetCore.Mvc;
using InfotecsWebAPI.Services;

namespace InfotecsWebAPI.Controllers;

/// <summary>
/// Controller for CSV file processing operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CsvController(ICsvProcessingService csvProcessingService, ILogger<CsvController> logger)
    : ControllerBase
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    private static readonly string[] AllowedExtensions = [".csv"];
    
    [EndpointSummary("Upload and process a CSV file containing timescale data.")]
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [
        ProducesResponseType(StatusCodes.Status200OK),
        ProducesResponseType(StatusCodes.Status400BadRequest),
        ProducesResponseType(StatusCodes.Status500InternalServerError)
    ]
    public async Task<IActionResult> UploadCsv(IFormFile? file)
    {
        if (IsFileInvalid(file, out var badRequest)) return badRequest!;
        
        try
        {
            await csvProcessingService.ProcessCsvFileAsync(file!.OpenReadStream(), file.FileName);
            
            return Ok(new { 
                message = "CSV file processed successfully", 
                fileName = file.FileName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Validation error processing CSV file: {FileName}", file?.FileName);
            return BadRequest(new { 
                error = "Validation error", 
                message = ex.Message,
                fileName = file?.FileName
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing CSV file: {FileName}", file?.FileName);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An unexpected error occurred while processing the file",
                fileName = file?.FileName
            });
        }
    }

    private bool IsFileInvalid(IFormFile? file, out IActionResult? badRequest)
    {
        if (file is null || file.Length == 0)
        {
            badRequest = BadRequest(new { error = "No file uploaded or file is empty." });
            return true;
        }
        if (file.Length > MaxFileSizeBytes)
        {
            badRequest = BadRequest(new { error = "File size exceeds the maximum limit of 50 MB." });
            return true;
        }
        if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())
            || (file.ContentType != "text/csv" && file.ContentType != "application/csv"))
        {
            badRequest = BadRequest(new { error = "Invalid file type. Only CSV files are allowed." });
            return true;
        }
        badRequest = null;
        return false;
    }
}
