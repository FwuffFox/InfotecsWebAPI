using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace InfotecsWebAPI.Models;

/// <summary>
/// Data Transfer Object for CSV value data.
/// Represents a single row from CSV with format: Date;ExecutionTime;Value
/// </summary>
public class ValueDTO
{
    /// <summary>
    /// Date in format yyyy-MM-ddTHH:mm:ss.fffZ
    /// </summary>
    required public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Execution time in seconds
    /// </summary>
    required public string ExecutionTime { get; set; }

    /// <summary>
    /// Value as floating point number
    /// </summary>
    required public string Value { get; set; }

    public class ValueDtoValidator : AbstractValidator<ValueDTO>
    {
        public ValueDtoValidator()
        {
            RuleFor(x => x.Date)
                .Must(date => date < DateTimeOffset.UtcNow)
                    .WithMessage("Date must be in the past.");

            RuleFor(x => x.ExecutionTime)
                .NotEmpty()
                    .WithMessage("Execution time is required.")
                .Matches(@"^\d+(\.\d+)?$")
                    .WithMessage("Execution time must be a valid number.")
                .Must(executionTime =>
                {
                    if (decimal.TryParse(executionTime, out var parsedExecutionTime))
                    {
                        return parsedExecutionTime >= 0;
                    }
                    return false;
                })
                    .WithMessage("Execution time must be non-negative.");

            RuleFor(x => x.Value)
                .NotEmpty()
                    .WithMessage("Value is required.")
                .Matches(@"^\d+(\.\d+)?$")
                    .WithMessage("Value must be a valid number.")
                .Must(value =>
                {
                    if (decimal.TryParse(value, out var parsedValue))
                    {
                        return parsedValue >= 0;
                    }
                    return false;
                })
                    .WithMessage("Value must be non-negative.");
        }
    }
}