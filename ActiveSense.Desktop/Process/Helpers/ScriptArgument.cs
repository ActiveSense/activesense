using System;

namespace ActiveSense.Desktop.HelperClasses
{
    public abstract class ScriptArgument
    {
        public required string Flag { get; set; }        // Command-line flag (e.g., "a" for "-a")
        public string Name { get; set; } = String.Empty;        // Display name
        public string Description { get; set; } = String.Empty;  // Description for UI
        
        public abstract string ToCommandLineArgument();
    }
    
    public class BoolArgument : ScriptArgument
    {
        public bool Value { get; set; }
        
        public override string ToCommandLineArgument()
        {
            if (Value == false)
            {
                return $"-{Flag} FALSE";
            }

            return $"-{Flag} TRUE";
        }
    }
    
    public class NumericArgument : ScriptArgument
    {
        public double Value { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        
        public override string ToCommandLineArgument()
        {
            return $"-{Flag} {Value}";
        }
    }
}