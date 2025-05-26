using System.Globalization;

namespace ActiveSense.Desktop.Infrastructure.Process.Helpers;

public abstract class ScriptArgument
{
    public required string Flag { get; set; } // Command-line flag (e.g., "a" for "--a")
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public abstract string ToCommandLineArgument();
}

public class BoolArgument : ScriptArgument
{
    public bool Value { get; set; }

    public override string ToCommandLineArgument()
    {
        if (Value == false) return $"--{Flag} FALSE";

        return $"--{Flag} TRUE";
    }
}

public class NumericArgument : ScriptArgument
{
    public double Value { get; set; }

    public double MinValue { get; set; }
    public double MaxValue { get; set; }

    public string DisplayValue
    {
        get => Value.ToString(CultureInfo.InvariantCulture);
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                Value = result;
        }
    }

    public override string ToCommandLineArgument()
    {
        return $"--{Flag} {Value.ToString(CultureInfo.InvariantCulture)}";
    }
}