using System.Globalization;
using InfotecsWebAPI.Models;

namespace InfotecsWebAPI.Mappers;

/// <summary>
///     Mapper for converting ValueDto to ValueEntity.
/// </summary>
public static class ValueDtoToValueEntityMapper
{
    /// <summary>
    ///     Maps ValueDto to ValueEntity.
    /// </summary>
    /// <param name="dto">The DTO to map</param>
    /// <param name="fileName">The name of the CSV file</param>
    /// <returns>Mapped ValueEntity</returns>
    /// <exception cref="ArgumentException">Thrown when DTO data is invalid</exception>
    public static ValueEntity ToEntity(ValueDto dto, string fileName)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

        var validator = new ValueDto.ValueDtoValidator();
        var validationResult = validator.Validate(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException($"Invalid DTO data: {errors}", nameof(dto));
        }

        return new ValueEntity
        {
            FileName = fileName,
            Date = dto.Date,
            ExecutionTime = decimal.Parse(dto.ExecutionTime, CultureInfo.InvariantCulture),
            Value = decimal.Parse(dto.Value, CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    ///     Maps multiple ValueDto objects to ValueEntity objects.
    /// </summary>
    /// <param name="dtos">Collection of DTOs to map</param>
    /// <param name="fileName">The name of the CSV file</param>
    /// <returns>Collection of mapped ValueEntity objects</returns>
    public static IEnumerable<ValueEntity> ToEntities(IEnumerable<ValueDto> dtos, string fileName)
    {
        ArgumentNullException.ThrowIfNull(dtos);

        return dtos.Select(dto => ToEntity(dto, fileName));
    }
}