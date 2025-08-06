using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace InfotecsWebAPI.Models;

/// <summary>
/// Data Transfer Object for CSV value data.
/// Represents a single row from CSV with format: Date;ExecutionTime;Value
/// </summary>
public class ValueDto
{
    /// <summary>
    /// Date in format yyyy-MM-ddTHH:mm:ss.fffZ
    /// </summary>
    public required DateTimeOffset Date { get; set; }

    /// <summary>
    /// Execution time in seconds
    /// </summary>
    public required string ExecutionTime { get; set; }

    /// <summary>
    /// Value as floating point number
    /// </summary>
    public required string Value { get; set; }

    public class ValueDtoValidator : AbstractValidator<ValueDto>
    {
        public ValueDtoValidator()
        {
            RuleFor(x => x.Date)
                .Must(date => date < DateTimeOffset.UtcNow)
                    .WithMessage(value => $"Date must be in the past. ({value.Date})");

            RuleFor(x => x.ExecutionTime)
                .NotEmpty()
                    .WithMessage("Execution time is required.")
                .Matches(@"^\d+(\.\d+)?$")
                    .WithMessage(value => $"Execution time must be a valid number. ({value.ExecutionTime})")
                .Must(executionTime =>
                {
                    if (decimal.TryParse(executionTime, out var parsedExecutionTime))
                    {
                        return parsedExecutionTime >= 0;
                    }
                    return false;
                })
                    .WithMessage(value => $"Execution time must be non-negative. ({value.ExecutionTime})");

            RuleFor(x => x.Value)
                .NotEmpty()
                    .WithMessage("Value is required.")
                .Matches(@"^\d+(\.\d+)?$")
                    .WithMessage(value => $"Value must be a valid number. ({value.Value})")
                .Must(value =>
                {
                    if (decimal.TryParse(value, out var parsedValue))
                    {
                        return parsedValue >= 0;
                    }
                    return false;
                })
                    .WithMessage(value => $"Value must be non-negative. ({value.Value})");
        }
    }
}