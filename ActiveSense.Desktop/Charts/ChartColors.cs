using System.Linq;
using SkiaSharp;

namespace ActiveSense.Desktop.Charts;

public class ChartColors
{

    public SKColor[] GetColorPalette(int count)
    {
        var predefinedColors = new[]
        {
            SKColor.Parse("#0072B2"),  // Blue - distinguishable in all forms of colorblindness
            SKColor.Parse("#E69F00"),  // Orange - visible to most colorblind types
            SKColor.Parse("#009E73"),  // Bluish green - distinguishable from blue/orange
            SKColor.Parse("#CC79A7"),  // Pink/purple - distinct luminance 
            SKColor.Parse("#D55E00"),  // Vermillion - high luminance contrast
            SKColor.Parse("#56B4E9"),  // Sky blue - different luminance than regular blue
            SKColor.Parse("#F0E442"),  // Yellow - high contrast with all others
            SKColor.Parse("#999999"),  // Gray - neutral with distinctive luminance
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