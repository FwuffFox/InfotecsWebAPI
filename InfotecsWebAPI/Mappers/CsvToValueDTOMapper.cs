using System.Globalization;
using CsvHelper.Configuration;
using InfotecsWebAPI.Models;

namespace InfotecsWebAPI.Mappers;

public sealed class CsvToValueDTOMapper : ClassMap<ValueDTO>
{
    public CsvToValueDTOMapper()
    {
        Map(m => m.Date).Name("Date")
            .TypeConverterOption.Format("yyyy-MM-ddTHH:mm:ss.fffZ");

        Map(m => m.ExecutionTime).Name("ExecutionTime")
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);

        Map(m => m.Value).Name("Value")
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
    }
}