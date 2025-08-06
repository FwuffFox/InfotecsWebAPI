using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InfotecsWebAPI.Models;

/// <summary>
///     Entity representing a single value entry from CSV file.
/// </summary>
[Table("Values")]
[Index(nameof(FileName))]
[Index(nameof(Date))]
[Index(nameof(FileName), nameof(Date))]
public class ValueEntity
{
    [Key] public int Id { get; set; }

    [Required] [MaxLength(255)] public string FileName { get; set; } = string.Empty;

    [Required] public DateTimeOffset Date { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,6)")]
    [Range(0.000000, double.MaxValue, ErrorMessage = "Execution time must be non-negative")]
    public decimal ExecutionTime { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,6)")]
    [Range(0, double.MaxValue, ErrorMessage = "Value must be non-negative.")]
    public decimal Value { get; set; }
}