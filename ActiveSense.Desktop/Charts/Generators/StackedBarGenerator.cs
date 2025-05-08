using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.ViewModels.Charts;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ActiveSense.Desktop.Charts.Generators;

public class StackedBarGenerator(ChartDataDTO[] chartDataDtos, ChartColors chartColors)
{
    public BarChartViewModel GenerateChart(string title, string description)
    {
        if (chartDataDtos == null || chartDataDtos.Length == 0)
        {
            return new BarChartViewModel
            {
                Series = Array.Empty<ISeries>(),
                XAxes = new[] { new Axis { Labels = new[] { "No Data" } } },
                YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 100 } }
            };
        }

        var allLabels = chartDataDtos
            .SelectMany(dto => dto.Labels)
            .Distinct()
            .ToArray();

        var series = new List<ISeries>();
        var colors = chartColors.GetColorPalette(chartDataDtos.Length);
        int colorIndex = 0;

        // Create a dictionary to track means per series
        var seriesMeans = new Dictionary<string, double>();

        foreach (var dto in chartDataDtos)
        {
            // Calculate the mean for this series
            double mean = dto.Data.Length > 0 ? dto.Data.Average() : 0;
            
            // Store the mean with the series name/title
            string seriesName = dto.Title ?? $"Series {colorIndex + 1}";
            seriesMeans[seriesName] = mean;

            // Add the bar series as before
            series.Add(new StackedColumnSeries<double>
            {
                Values = dto.Data,
                Stroke = null,
                Fill = new SolidColorPaint(colors[colorIndex++]),
                MaxBarWidth = 15,
                Name = seriesName
            });
        }

        // Now add mean lines for each series
        colorIndex = 0;
        foreach (var kvp in seriesMeans)
        {
            var seriesName = kvp.Key;
            var meanValue = kvp.Value;
            var meanValues = Enumerable.Repeat(meanValue, allLabels.Length).ToArray();
            
            // Use a darker shade of the same color for the mean line
            var seriesColor = colors[colorIndex++];
            var darkerColor = new SKColor(
                (byte)(seriesColor.Red * 0.7f), 
                (byte)(seriesColor.Green * 0.7f), 
                (byte)(seriesColor.Blue * 0.7f)
            );
            
            series.Add(new LineSeries<double>
            {
                Values = meanValues,
                Stroke = new SolidColorPaint(darkerColor, 2),
                Fill = null,
                GeometrySize = 0,
                Name = $"Durchschnitt {seriesName}",
                LineSmoothness = 0,
                IsVisibleAtLegend = false // Optional: hide from legend to avoid clutter
            });
        }

        var xAxis = new Axis
        {
            Labels = allLabels,
            LabelsRotation = -45,
            ForceStepToMin = true,
            MinStep = 1
        };

        return new BarChartViewModel
        {
            Title = title,
            Description = description,
            Series = series.ToArray(),
            XAxes = new[] { xAxis },
            YAxes = new[] { new Axis() }
        };
    }
}