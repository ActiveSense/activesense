using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Export.Interfaces;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.ViewModels.Charts;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace ActiveSense.Desktop.Export.Implementations;

public class ChartRenderer(ChartColors chartColors) : IChartRenderer
{
    public byte[] RenderSleepDistributionChart(IChartDataProvider chartProvider)
    {
        var dto = chartProvider.GetSleepDistributionChartData();
        var pieChartGenerator = new PieChartGenerator(dto, chartColors);
        var pieChartViewModel = pieChartGenerator.GenerateChart("Schlafverteilung", "");

        foreach (var series in pieChartViewModel.PieSeries.Cast<PieSeries<double>>())
        {
            series.DataLabelsSize = 14;
            series.DataLabelsPosition = PolarLabelsPosition.Middle;
            series.DataLabelsPaint = new SolidColorPaint(SKColors.White);
        }

        return RenderPieChart(pieChartViewModel);
    }
    
    public byte[] RenderMovementPatternChart(IChartDataProvider chartProvider)
    {
        var dto = chartProvider.GetMovementPatternChartData();
        var pieChartGenerator = new PieChartGenerator(dto, chartColors);
        var pieChartViewModel = pieChartGenerator.GenerateChart("Aktivitätsverteilung", "");

        foreach (var series in pieChartViewModel.PieSeries.Cast<PieSeries<double>>())
        {
            series.DataLabelsSize = 14;
            series.DataLabelsPosition = PolarLabelsPosition.Middle;
            series.DataLabelsPaint = new SolidColorPaint(SKColors.White);
        }

        return RenderPieChart(pieChartViewModel);
    }

    public byte[] RenderStepsChart(IChartDataProvider chartProvider)
    {
        var dto = chartProvider.GetStepsChartData();
        var barChartGenerator = new BarChartGenerator([dto], chartColors);
        var barChartViewModel = barChartGenerator.GenerateChart("Schritte pro Tag", "");

        foreach (var series in barChartViewModel.Series.OfType<ColumnSeries<double>>())
        {
            series.DataLabelsSize = 0;
        }

        return RenderCartesianChart(barChartViewModel);
    }
    
    public byte[] RenderSleepWithEfficiencyChart(IChartDataProvider chartProvider)
    {
        var sleepDto = chartProvider.GetTotalSleepTimePerDayChartData();
        var efficiencyDto = chartProvider.GetSleepEfficiencyChartData();
        
        var barChartGenerator = new BarChartGenerator([sleepDto], chartColors, [efficiencyDto]);
        var barChartViewModel = barChartGenerator.GenerateChart("Schlafzeit mit Effizienz", "");

        ConfigureBarLineChart(barChartViewModel, "Schlafzeit (h)", "Schlafeffizienz (%)");

        return RenderCartesianChart(barChartViewModel);
    }
    
    public byte[] RenderStepsWithSleepEfficiencyChart(IChartDataProvider chartProvider)
    {
        // Create separate lists for bar and line data
        var barData = new List<ChartDataDTO> { chartProvider.GetStepsChartData() };
        var lineData = new List<ChartDataDTO> { chartProvider.GetSleepEfficiencyChartData() };
        
        var chartGenerator = new BarChartGenerator(barData.ToArray(), chartColors, lineData.ToArray());
        var barChartViewModel = chartGenerator.GenerateChart("Schritte & Schlafeffizienz", "");

        ConfigureBarLineChart(barChartViewModel, "Schritte", "Schlafeffizienz (%)");

        return RenderCartesianChart(barChartViewModel);
    }

    public byte[] RenderActivityDistributionChart(IChartDataProvider chartProvider)
    {
        var dtos = chartProvider.GetActivityDistributionChartData().ToArray();
        var stackedBarGenerator = new StackedBarGenerator(dtos, chartColors);
        var chartViewModel = stackedBarGenerator.GenerateChart("Aktivitätsverteilung", "");

        foreach (var series in chartViewModel.Series.OfType<StackedColumnSeries<double>>())
        {
            series.DataLabelsSize = 0;
            series.DataLabelsPosition = DataLabelsPosition.Middle;
            series.DataLabelsPaint = new SolidColorPaint(SKColors.White);
        }

        return RenderCartesianChart(chartViewModel);
    }
    
    private void ConfigureBarLineChart(BarChartViewModel barChartViewModel, string barSeriesName, string lineSeriesName)
    {
        foreach (var series in barChartViewModel.Series.OfType<ColumnSeries<double>>())
        {
            series.DataLabelsSize = 0;
            series.Name = barSeriesName;
        }
        
        foreach (var series in barChartViewModel.Series.OfType<LineSeries<double>>())
        {
            series.DataLabelsSize = 0;
            series.GeometrySize = 6;
            series.LineSmoothness = 0;
            series.Name = lineSeriesName;
        }
    }
    
    private byte[] RenderPieChart(PieChartViewModel viewModel)
    {
        var pieChart = new SKPieChart
        {
            Series = viewModel.PieSeries,
            Width = 500,
            Height = 300,
            LegendPosition = LegendPosition.Right,
            TooltipPosition = TooltipPosition.Right
        };

        using var image = pieChart.GetImage();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
    
    private byte[] RenderCartesianChart(BarChartViewModel viewModel)
    {
        var barChart = new SKCartesianChart
        {
            Series = viewModel.Series,
            XAxes = viewModel.XAxes,
            YAxes = viewModel.YAxes,
            Width = 500,
            Height = 300,
            LegendPosition = LegendPosition.Bottom
        };

        using var image = barChart.GetImage();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}