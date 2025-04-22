using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.Charts.Generators;

public class BarChartGenerator(ChartDataDTO[] barData, ChartColors chartColors, ChartDataDTO[] lineData = null)
{
    /// <summary>
    /// Normalizes chart data to make sure that all series have values for each label
    /// </summary>
    /// <returns>An array of values corresponding to each label in allLabels</returns>
    public double[] NormalizeChartData(ChartDataDTO dto, string[] allLabels)
    {
        var valueMap = new Dictionary<string, double>();

        // Initialize all labels with zero
        foreach (var label in allLabels)
        {
            valueMap[label] = 0;
        }

        // Map the actual values from the DTO to their corresponding labels
        for (int i = 0; i < dto.Labels.Length; i++)
        {
            if (i < dto.Data.Length)
            {
                valueMap[dto.Labels[i]] = dto.Data[i];
            }
        }

        // Create a new array with values in the same order as allLabels
        return allLabels
            .Select(label => valueMap[label])
            .ToArray();
    }

    public BarChartViewModel GenerateChart(string title, string description)
    {
        if (barData == null || barData.Length == 0)
        {
            return new BarChartViewModel
            {
                Series = Array.Empty<ISeries>(),
                XAxes = new[] { new Axis { Labels = new[] { "No Data" } } },
                YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 100 } }
            };
        }

        var allLabels = barData
            .SelectMany(dto => dto.Labels)
            .Concat(lineData?.SelectMany(dto => dto.Labels) ?? Enumerable.Empty<string>())
            .Distinct()
            .ToArray();

        var series = new List<ISeries>();
        var colors = chartColors.GetColorPalette(barData.Length);
        int colorIndex = 0;

        foreach (var dto in barData)
        {
            var normalizedValues = NormalizeChartData(dto, allLabels);

            series.Add(new ColumnSeries<double>
            {
                Values = normalizedValues,
                Stroke = null,
                Fill = new SolidColorPaint(colors[colorIndex++]),
                MaxBarWidth = 15,
                Name = dto.Title ?? $"Series {colorIndex}"
            });
        }

        // Add line series if provided
        if (lineData != null)
        {
            foreach (var dto in lineData)
            {
                var normalizedValues = NormalizeChartData(dto, allLabels);

                series.Add(new LineSeries<double>
                {
                    Values = normalizedValues,
                    Stroke = new SolidColorPaint(SKColors.Red, 2),
                    Fill = null,
                    GeometrySize = 0,
                    Name = dto.Title ?? "Line Series",
                    LineSmoothness = 0
                });
            }
        }

        // Add mean line
        var allValues = barData.SelectMany(dto => dto.Data);
        double meanValue = allValues.Any() ? allValues.Average() : 0;
        var meanValues = Enumerable.Repeat(meanValue, allLabels.Length).ToArray();
        series.Add(new LineSeries<double>
        {
            Values = meanValues,
            Stroke = new SolidColorPaint(SKColors.Gray, 2),
            Fill = null,
            GeometrySize = 0,
            Name = "Durchschnitt",
            LineSmoothness = 0
        });

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