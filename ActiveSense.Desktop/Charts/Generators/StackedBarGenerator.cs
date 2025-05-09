using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.ViewModels.Charts;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ActiveSense.Desktop.Charts.Generators;

public class StackedBarGenerator(ChartDataDTO[] chartDataDtos, ChartColors chartColors)
{
    public BarChartViewModel GenerateChart(string title, string description)
    {
        if (chartDataDtos.Length == 0)
        {
            return new BarChartViewModel
            {
                Series = [],
                XAxes = new[] { new Axis { Labels = ["No Data"] } },
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

        // track means per series
        var seriesMeans = new Dictionary<string, double>();

        foreach (var dto in chartDataDtos)
        {
            double mean = dto.Data.Length > 0 ? dto.Data.Average() : 0;
            
            string seriesName = dto.Title;
            seriesMeans[seriesName] = mean;

            series.Add(new StackedColumnSeries<double>
            {
                Values = dto.Data,
                Stroke = null,
                Fill = new SolidColorPaint(colors[colorIndex++]),
                MaxBarWidth = 15,
                Name = seriesName
            });
        }

        // add mean lines for each series
        colorIndex = 0;
        foreach (var kvp in seriesMeans)
        {
            var seriesName = kvp.Key;
            var meanValue = kvp.Value;
            var meanValues = Enumerable.Repeat(meanValue, allLabels.Length).ToArray();
            
            var seriesColor = colors[colorIndex++];
            
            series.Add(new LineSeries<double>
            {
                Values = meanValues,
                Stroke = new SolidColorPaint(seriesColor, 2),
                Fill = null,
                GeometrySize = 0,
                GeometryFill = null,
                GeometryStroke = null,
                Name = $"Durchschnitt {seriesName}",
                LineSmoothness = 0,
                IsVisibleAtLegend = false,
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