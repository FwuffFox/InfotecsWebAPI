using System.ComponentModel;
using System.Diagnostics;
using InfotecsWebAPI.Models;
using InfotecsWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfotecsWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValuesController(ActivitySource activitySource, IValueService valueService, ILogger logger) : ControllerBase
{
    [HttpGet("values")]
    [EndpointSummary("Gets last amount of values from the Results table based on a provided file name.")]
    [ProducesResponseType(typeof(IEnumerable<ValueEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ValueEntity>>> GetValuesByFileName(
        [FromQuery] [Description("File name to filter values by")]
        string fileName,
        [FromQuery] [Description("Number of values to return (default is 10)")]
        int count = 10,
        [FromQuery] [Description("Sort date by descending order (default is true)")]
        bool sortDescending = true)
    {
        try
        {
            using var activity = activitySource.StartActivity();

            var results = await valueService.GetLastValuesAsync(fileName, sortDescending, count);
            
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving values for file: {FileName}", fileName);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}