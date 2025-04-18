using System.Linq;
using SkiaSharp;

namespace ActiveSense.Desktop.Charts.Generators;

public class ChartColors
{
    
    public SKColor[] GetColorPalette(int count)
    {
        var predefinedColors = new[]
        {
            SKColors.CornflowerBlue,
            SKColors.Orange,
            SKColors.ForestGreen,
            SKColors.Crimson,
            SKColors.Purple,
            SKColors.Gold,
            SKColors.Teal,
            SKColors.DarkSlateBlue
        };

        if (count <= predefinedColors.Length)
        {
            return predefinedColors.Take(count).ToArray();
        }

        // Generate additional colors if needed
        var colors = new SKColor[count];
        for (int i = 0; i < count; i++)
        {
            if (i < predefinedColors.Length)
            {
                colors[i] = predefinedColors[i];
            }
            else
            {
                float hue = (360f / (count - predefinedColors.Length)) * (i - predefinedColors.Length);
                colors[i] = SKColor.FromHsl(hue, 80, 60);
            }
        }

        return colors;
    }
}