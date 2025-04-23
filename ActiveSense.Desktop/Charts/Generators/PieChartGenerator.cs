using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.ViewModels.Charts;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ActiveSense.Desktop.Charts.Generators
{
    public class PieChartGenerator(ChartDataDTO chartDataDto, ChartColors chartColors)
    {
        public PieChartViewModel GenerateChart(string title, string description)
        {
            var series = new List<ISeries>();
            
            var colors = chartColors.GetColorPalette(chartDataDto.Labels.Length);
            
            for (int i = 0; i < chartDataDto.Labels.Length; i++)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new[] { chartDataDto.Data[i] },
                    Name = chartDataDto.Labels[i],
                    Fill = new SolidColorPaint(colors[i]),
                });
            }
            
            return new PieChartViewModel
            {
                Title = title,
                Description = description,
                PieSeries = series.ToArray()
            };
        }
    }
}