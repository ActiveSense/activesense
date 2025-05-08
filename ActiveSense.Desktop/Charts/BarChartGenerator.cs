using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.Charts;

public class BarChartGenerator(ChartDataDTO[]? barData, ChartColors chartColors, ChartDataDTO[]? lineData = null)
{
    public double[] NormalizeChartData(ChartDataDTO dto, string[] allLabels)
    {
        var valueMap = new Dictionary<string, double>();

        foreach (var label in allLabels) valueMap[label] = 0;

        for (var i = 0; i < dto.Labels.Length; i++)
            if (i < dto.Data.Length)
                valueMap[dto.Labels[i]] = dto.Data[i];

        return allLabels
            .Select(label => valueMap[label])
            .ToArray();
    }

    public BarChartViewModel GenerateChart(string title, string description)
    {
        if ((barData == null || barData.Length == 0) && (lineData == null || lineData.Length == 0))
            return new BarChartViewModel
            {
                Series = Array.Empty<ISeries>(),
                XAxes = new[] { new Axis { Labels = new[] { "No Data" } } },
                YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 100 } }
            };

        var allLabels = (barData?.SelectMany(dto => dto.Labels) ?? Enumerable.Empty<string>())
            .Concat(lineData?.SelectMany(dto => dto.Labels) ?? Enumerable.Empty<string>())
            .Distinct()
            .ToArray();

        var series = new List<ISeries>();
        var yAxes = new List<ICartesianAxis>();

        // Create primary Y axis for bar data (left side)
        var primaryAxis = new Axis
        {
            NameTextSize = 12,
            TextSize = 10,
            Position = AxisPosition.Start
        };
        yAxes.Add(primaryAxis);

        // Create secondary Y axis for line data (right side) if needed
        if (lineData != null && lineData.Length > 0 && barData != null && barData.Length > 0)
        {
            var secondaryColor = SKColors.Red;
            if (lineData.Length > 0 && !string.IsNullOrEmpty(lineData[0].Title))
            {
                var secondaryAxis = new Axis
                {
                    Name = lineData[0].Title,
                    NameTextSize = 12,
                    NamePaint = new SolidColorPaint(secondaryColor),
                    TextSize = 10,
                    LabelsPaint = new SolidColorPaint(secondaryColor),
                    TicksPaint = new SolidColorPaint(secondaryColor),
                    SubticksPaint = new SolidColorPaint(secondaryColor),
                    DrawTicksPath = true,
                    ShowSeparatorLines = false,
                    Position = AxisPosition.End
                };
                yAxes.Add(secondaryAxis);
            }
        }

        // Add bar series if provided
        if (barData != null && barData.Length > 0)
        {
            var colors = chartColors.GetColorPalette(barData.Length);
            var colorIndex = 0;

            foreach (var dto in barData)
            {
                var normalizedValues = NormalizeChartData(dto, allLabels);

                series.Add(new ColumnSeries<double>
                {
                    Values = normalizedValues,
                    Stroke = null,
                    Fill = new SolidColorPaint(colors[colorIndex++]),
                    MaxBarWidth = 15,
                    Name = dto.Title ?? $"Series {colorIndex}",
                    ScalesYAt = 0 // Scale using the first Y axis
                });
            }

            // Add mean line if only bar data is provided
            if (lineData == null || lineData.Length == 0)
            {
                var allValues = barData.SelectMany(dto => dto.Data);
                var meanValue = allValues.Any() ? allValues.Average() : 0;
                var meanValues = Enumerable.Repeat(meanValue, allLabels.Length).ToArray();
                series.Add(new LineSeries<double>
                {
                    Values = meanValues,
                    Stroke = new SolidColorPaint(SKColors.Gray, 2),
                    Fill = null,
                    GeometrySize = 0,
                    Name = "Durchschnitt",
                    LineSmoothness = 0,
                    ScalesYAt = 0 // Scale using the first Y axis
                });
            }
        }

        // Add line series if provided
        if (lineData != null && lineData.Length > 0)
        {
            var lineColors = chartColors.GetColorPalette(lineData.Length);
            var lineColorIndex = 0;

            // If we have only line data with no bars, scale it to first axis
            // Otherwise, scale to second axis
            var scaleYAt = barData == null || barData.Length == 0 ? 0 : 1;

            foreach (var dto in lineData)
            {
                var normalizedValues = NormalizeChartData(dto, allLabels);
                var lineColor = lineData.Length > 1 ? lineColors[lineColorIndex++] : SKColors.Red;

                series.Add(new LineSeries<double>
                {
                    Values = normalizedValues,
                    Stroke = new SolidColorPaint(lineColor, 2),
                    Fill = null,
                    GeometrySize = 5,
                    GeometryStroke = new SolidColorPaint(lineColor, 2),
                    Name = dto.Title ?? "Line Series",
                    LineSmoothness = 0.5,
                    ScalesYAt = scaleYAt
                });
            }
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
            XAxes = [xAxis],
            YAxes = yAxes.ToArray(),
            LegendPosition = LegendPosition.Right,  // Position the legend on the right
        };
    }
}