using System.ComponentModel;
using System.Diagnostics;
using InfotecsWebAPI.Models;
using InfotecsWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfotecsWebAPI.Controllers;

/// <summary>
///     Controller for viewing processed data and results.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ResultsController(
    ILogger<ResultsController> logger,
    IResultService resultService,
    ActivitySource activitySource) : ControllerBase
{
    [HttpGet("results")]
    [EndpointSummary("Gets filtered results from the Results table based on provided criteria.")]
    [ProducesResponseType(typeof(IEnumerable<ResultEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ResultEntity>>> GetFilteredResults(
        [FromQuery] [Description("Filter by file name (optional)")]
        string? fileName = null,
        [FromQuery] [Description("Minimum start time for filtering (optional)")]
        DateTimeOffset? minStartTime = null,
        [FromQuery] [Description("Maximum start time for filtering (optional)")]
        DateTimeOffset? maxStartTime = null,
        [FromQuery] [Description("Minimum average value for filtering (optional)")]
        decimal? minAvgValue = null,
        [FromQuery] [Description("Maximum average value for filtering (optional)")]
        decimal? maxAvgValue = null,
        [FromQuery] [Description("Minimum average execution time for filtering (optional)")]
        decimal? minAvgExecutionTime = null,
        [FromQuery] [Description("Maximum average execution time for filtering (optional)")]
        decimal? maxAvgExecutionTime = null)
    {
        try
        {
            using var activity = activitySource.StartActivity();

            var results = await resultService.GetFilteredResultsAsync(
                fileName,
                minStartTime,
                maxStartTime,
                minAvgValue,
                maxAvgValue,
                minAvgExecutionTime,
                maxAvgExecutionTime);

            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving filtered results");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}