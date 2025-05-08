using CsvHelper.Configuration.Attributes;

namespace ActiveSense.Desktop.Models;
public class ActivityRecord
{
    [Name("Day.Number")]
    public required string Day { get; set; }

    [Name("Steps")]
    public required string Steps { get; set; }

    [Name("Non_Wear")]
    public required string NonWear { get; set; }

    [Name("Sleep")]
    public required string Sleep { get; set; }

    [Name("Sedentary")]
    public required string Sedentary { get; set; }

    [Name("Light")]
    public required string Light { get; set; }

    [Name("Moderate")]
    public required string Moderate { get; set; }

    [Name("Vigorous")]
    public required string Vigorous { get; set; }
}
