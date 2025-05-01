using System;

namespace ActiveSense.Desktop.Models;

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
            
            int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
            
            float darkenFactor = 0.6f;
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
