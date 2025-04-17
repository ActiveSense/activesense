using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.ViewModels;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.ViewModels.Charts;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

public class BarChartGenerator
{
    private readonly ChartDataDTO[] _chartDataDtos;
    private readonly ICartesianAxis _xAxis;
    private readonly ICartesianAxis _yAxis;
    
    public BarChartGenerator(ChartDataDTO[] chartDataDtos)
    {
        _chartDataDtos = chartDataDtos;
        _xAxis = new Axis();
        _yAxis = new Axis();
    }
    
    public BarChartViewModel GenerateChart()
    {
        if (_chartDataDtos == null || _chartDataDtos.Length == 0)
        {
            return new BarChartViewModel
            {
                Series = Array.Empty<ISeries>(),
                XAxes = new[] { _xAxis },
                YAxes = new[] { _yAxis }
            };
        }
        
        // 1. Collect all unique labels across all DTOs
        var allLabels = _chartDataDtos
            .SelectMany(dto => dto.Labels)
            .Distinct()
            .OrderBy(label => label)
            .ToArray();
            
        // 2. Create series list and color palette
        var series = new List<ISeries>();
        var colors = GetColorPalette(_chartDataDtos.Length);
        int colorIndex = 0;
        
        // 3. Process each DTO
        foreach (var dto in _chartDataDtos)
        {
            // Create a mapping of labels to data values
            var valueMap = new Dictionary<string, double>();
            
            // Initialize all labels with zero values
            foreach (var label in allLabels)
            {
                valueMap[label] = 0;
            }
            
            // Fill in actual values where they exist
            for (int i = 0; i < dto.Labels.Length; i++)
            {
                if (i < dto.Data.Length)
                {
                    valueMap[dto.Labels[i]] = dto.Data[i];
                }
            }
            
            // Extract normalized values in the same order as allLabels
            var normalizedValues = allLabels
                .Select(label => valueMap[label])
                .ToArray();
                
            // Add series for this DTO
            series.Add(new ColumnSeries<double>
            {
                Values = normalizedValues,
                Stroke = null,
                Fill = new SolidColorPaint(colors[colorIndex++]),
                MaxBarWidth = 15,
                Name = dto.Title ?? $"Series {colorIndex}"
            });
        }
        
        // 4. Calculate mean values across all series for each label
        if (_chartDataDtos.Length > 1)
        {
            var meanValues = new double[allLabels.Length];
            
            for (int i = 0; i < allLabels.Length; i++)
            {
                double sum = 0;
                int count = 0;
                
                foreach (var dto in _chartDataDtos)
                {
                    int labelIndex = Array.IndexOf(dto.Labels, allLabels[i]);
                    if (labelIndex >= 0 && labelIndex < dto.Data.Length)
                    {
                        sum += dto.Data[labelIndex];
                        count++;
                    }
                }
                
                meanValues[i] = count > 0 ? sum / count : 0;
            }
            
            // Add mean line
            series.Add(new LineSeries<double>
            {
                Values = meanValues,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Fill = null,
                GeometrySize = 0,
                Name = "Average",
                LineSmoothness = 0
            });
        }
        
        // 5. Update X-Axis with normalized labels
        var xAxis = new Axis
        {
            Labels = allLabels,
            LabelsRotation = -45
        };
        
        // 6. Create view model
        return new BarChartViewModel
        {
            Series = series.ToArray(),
            XAxes = new[] { xAxis },
            YAxes = new[] { _yAxis }
        };
    }
    
    private SKColor[] GetColorPalette(int count)
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