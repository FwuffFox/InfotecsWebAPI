using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InfotecsWebAPI.Models;

/// <summary>
/// Entity representing aggregated results calculated from CSV file data.
/// </summary>
[Table("Results")]
[Index(nameof(FileName), IsUnique = true)]
[Index(nameof(MinStartTime))]
[Index(nameof(AvgValue))]
[Index(nameof(AvgExecutionTime))]
public class ResultEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Time delta in seconds (max Date - min Date).
    /// </summary>
    [Required]
    public long TimeDelta { get; set; }

    /// <summary>
    /// Minimum start time as the first operation start moment.
    /// </summary>
    [Required]
    public DateTimeOffset MinStartTime { get; set; }

    /// <summary>
    /// Average execution time.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal AvgExecutionTime { get; set; }

    /// <summary>
    /// Average value.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal AvgValue { get; set; }

    /// <summary>
    /// Median value.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal MedianValue { get; set; }

    /// <summary>
    /// Maximum value.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal MaxValue { get; set; }

    /// <summary>
    /// Minimum value.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal MinValue { get; set; }
}
