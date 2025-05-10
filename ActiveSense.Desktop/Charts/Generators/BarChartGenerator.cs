using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.Charts.Generators;

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

        var allLabels = (barData?.SelectMany(dto => dto.Labels) ?? [])
            .Concat(lineData?.SelectMany(dto => dto.Labels) ?? [])
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
                var rawValues = dto.Data;
                var color = colors[colorIndex++];

                series.Add(new ColumnSeries<double>
                {
                    Values = normalizedValues,
                    Stroke = null,
                    Fill = new SolidColorPaint(color),
                    MaxBarWidth = 15,
                    Name = dto.Title ?? $"Series {colorIndex}",
                    ScalesYAt = 0
                });

                var meanValue = rawValues.Any() ? rawValues.Average() : 0;
                var meanValues = Enumerable.Repeat(meanValue, allLabels.Length).ToArray();
                series.Add(new LineSeries<double>
                {
                    Values = meanValues,
                    Stroke = new SolidColorPaint(color, 2),
                    Fill = null,
                    GeometrySize = 0,
                    GeometryFill = null,
                    GeometryStroke = null,
                    IsHoverable = false,
                    IsVisibleAtLegend = false,
                    Name = $"Durchschnitt {dto.Title ?? $"Series {colorIndex}"}",
                    LineSmoothness = 0,
                    ScalesYAt = 0
                });
            }
        }

        // Add line series if provided
        if (lineData != null && lineData.Length > 0)
        {
            var lineColors = chartColors.GetColorPalette(lineData.Length);
            var lineColorIndex = 0;

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
                    Name = dto.Title,
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
            LegendPosition = LegendPosition.Right,
        };
    }
}