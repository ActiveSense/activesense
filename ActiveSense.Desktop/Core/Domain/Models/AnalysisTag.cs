using System;

namespace ActiveSense.Desktop.Core.Domain.Models;

public class AnalysisTag
{
    public AnalysisTag(string name, string color = "#000000")
    {
        Name = name;
        Color = color;
        TextColor = GetDarkerColor(color);
    }

    public string Name { get; set; }
    public string Color { get; set; }
    public string TextColor { get; set; }

    private string GetDarkerColor(string hexColor)
    {
        try
        {
            hexColor = hexColor.TrimStart('#');

            var r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            var b = Convert.ToInt32(hexColor.Substring(4, 2), 16);

            var darkenFactor = 0.6f;
            r = (int)(r * darkenFactor);
            g = (int)(g * darkenFactor);
            b = (int)(b * darkenFactor);

            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            return $"#{r:X2}{g:X2}{b:X2}";
        }
        catch
        {
            return "#000000";
        }
    }
}