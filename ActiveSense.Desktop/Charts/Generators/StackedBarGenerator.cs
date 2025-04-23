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
            .ToArray();

        var series = new List<ISeries>();
        var colors = chartColors.GetColorPalette(chartDataDtos.Length);
        int colorIndex = 0;

        foreach (var dto in chartDataDtos)
        {
            series.Add(new StackedColumnSeries<double>
            {
                Values = dto.Data,
                Stroke = null,
                Fill = new SolidColorPaint(colors[colorIndex++]),
                MaxBarWidth = 15,
                Name = dto.Title
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