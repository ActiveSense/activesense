using System;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace ActiveSense.Desktop.Sensors;

public class GeneActiveExporter(ChartColors chartColors, AnalysisSerializer serializer) : IExporter
{
    public SensorTypes SupportedType => SensorTypes.GENEActiv;

    public async Task<bool> ExportAsync(IAnalysis analysis, string outputPath)
    {
        if (analysis is not (IActivityAnalysis activityAnalysis and ISleepAnalysis sleepAnalysis and IChartDataProvider chartProvider))
        {
            Console.WriteLine("Analysis does not provide required capabilities for GeneActive export");
            return false;
        }

        try
        {
            Settings.License = LicenseType.Community;
            var exportData = serializer.ExportToBase64(analysis);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header
                    page.Header()
                        .Text("ActiveSense Analysis Report")
                        .Bold()
                        .FontSize(16);

                    // Content
                    page.Content()
                        .Column(column =>
                        {
                            // Title section
                            column.Item().PaddingVertical(10)
                                .Text($"Analysis: {analysis.FileName}")
                                .Bold()
                                .FontSize(14);

                            // Sleep Data Section
                            column.Item().PaddingTop(10)
                                .Text("Schlafdaten")
                                .Bold()
                                .FontSize(16);

                            if (sleepAnalysis.SleepRecords == null || sleepAnalysis.SleepRecords.Count == 0)
                            {
                                column.Item().Text("No sleep data available").Italic().FontSize(12);
                            }
                            else
                            {
                                // Add sleep data table or charts
                                column.Item().PaddingVertical(10)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(120);
                                            columns.RelativeColumn();
                                        });

                                        AddTableRow(table, "Average Sleep Time",
                                            $"{sleepAnalysis.AverageSleepTime / 3600:F2} hours");
                                        AddTableRow(table, "Average Sleep Efficiency",
                                            $"{sleepAnalysis.SleepEfficiency.Average():F1}%");
                                    });

                                column.Item().PaddingVertical(10)
                                    .Height(200)
                                    .Image(GeneratePieChartImage(chartProvider));
                            }

                            // Activity Data Section
                            column.Item().PaddingTop(20)
                                .Text("Aktivitätsdaten")
                                .Bold()
                                .FontSize(16);

                            if (activityAnalysis.ActivityRecords == null || !activityAnalysis.ActivityRecords.Any())
                            {
                                column.Item().Text("No activity data available").Italic().FontSize(12);
                            }
                            else
                            {
                                // Add activity data table or charts
                                column.Item().PaddingVertical(10)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(120);
                                            columns.RelativeColumn();
                                        });

                                        AddTableRow(table, "Average Steps",
                                            $"{activityAnalysis.StepsPerDay.Average():F0} steps");
                                        AddTableRow(table, "Average Light Activity",
                                            $"{activityAnalysis.AverageLightActivity / 60:F1} minutes");
                                    });

                                column.Item().PaddingVertical(10)
                                    .Height(200)
                                    .Image(GenerateStepsChartImage(chartProvider));

                                // insert encoded data 
                                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                    .PaddingTop(10).PaddingBottom(5)
                                    .Text("ANALYSIS_DATA_BEGIN")
                                    .FontSize(6)
                                    .FontColor(Colors.Grey.Medium);

                                column.Item().Text(exportData)
                                    .FontSize(4)
                                    .FontColor(Colors.Grey.Medium);

                                column.Item().Text("ANALYSIS_DATA_END")
                                    .FontSize(6)
                                    .FontColor(Colors.Grey.Medium);
                            }
                        });


                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated: ");
                            text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                            text.Span(" | Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf(outputPath);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PDF export failed: {ex.Message}");
            return false;
        }
    }

    private void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Text(label).Bold();
        table.Cell().Text(value);
    }

    private byte[] GeneratePieChartImage(IChartDataProvider analysis)
    {
        try
        {
            var dto = analysis.GetSleepChartData();
            var pieChartGenerator = new PieChartGenerator(dto, chartColors);
            var pieChartViewModel = pieChartGenerator.GenerateChart("Sleep Distribution", "");

            foreach (var series in pieChartViewModel.PieSeries.Cast<PieSeries<double>>())
            {
                series.DataLabelsSize = 14;
                series.DataLabelsPosition = PolarLabelsPosition.Middle;
                series.DataLabelsPaint = new SolidColorPaint(SKColors.White);
            }

            // Create a SkiaSharp chart from the view model
            var pieChart = new SKPieChart
            {
                Series = pieChartViewModel.PieSeries,
                Width = 700,
                Height = 400,
                LegendPosition = LegendPosition.Right,
                TooltipPosition = TooltipPosition.Right
            };

            // Get the image
            using var image = pieChart.GetImage();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating pie chart: {ex.Message}");
            return CreateErrorImage("Failed to generate pie chart");
        }
    }

    private byte[] GenerateStepsChartImage(IChartDataProvider analysis)
    {
        try
        {
            // Use your existing generator
            var dto = analysis.GetStepsChartData();
            var barChartGenerator = new BarChartGenerator(new[] { dto }, chartColors);
            var barChartViewModel = barChartGenerator.GenerateChart("Steps per Day", "");

            // Modify column series to display values
            foreach (var series in barChartViewModel.Series.OfType<ColumnSeries<double>>())
            {
                series.DataLabelsSize = 12;
                series.DataLabelsPosition = DataLabelsPosition.Top;
                series.DataLabelsPaint = new SolidColorPaint(SKColors.Black);
            }

            // Create a SkiaSharp chart from the view model
            var barChart = new SKCartesianChart
            {
                Series = barChartViewModel.Series,
                XAxes = barChartViewModel.XAxes,
                YAxes = barChartViewModel.YAxes,
                Width = 700,
                Height = 400,
                LegendPosition = LegendPosition.Bottom
            };

            // Get the image
            using var image = barChart.GetImage();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating bar chart: {ex.Message}");
            return CreateErrorImage("Failed to generate steps chart");
        }
    }

    private byte[] GenerateActivityDistributionChartImage(IChartDataProvider analysis)
    {
        try
        {
            var dtos = analysis.GetActivityDistributionChartData().ToArray();
            var stackedBarGenerator = new StackedBarGenerator(dtos, chartColors);
            var chartViewModel = stackedBarGenerator.GenerateChart("Activity Distribution", "");

            foreach (var series in chartViewModel.Series.OfType<StackedColumnSeries<double>>())
            {
                series.DataLabelsSize = 10;
                series.DataLabelsPosition = DataLabelsPosition.Middle;
                series.DataLabelsPaint = new SolidColorPaint(SKColors.White);
                series.DataLabelsRotation = -90; // Rotate labels to fit better in narrow columns
            }

            var barChart = new SKCartesianChart
            {
                Series = chartViewModel.Series,
                XAxes = chartViewModel.XAxes,
                YAxes = chartViewModel.YAxes,
                Width = 700,
                Height = 400,
                LegendPosition = LegendPosition.Bottom
            };

            using var image = barChart.GetImage();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating stacked bar chart: {ex.Message}");
            return CreateErrorImage("Failed to generate activity distribution chart");
        }
    }

    private byte[] CreateErrorImage(string errorMessage)
    {
        var width = 700;
        var height = 400;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            TextSize = 20,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        canvas.DrawText(errorMessage, width / 2, height / 2, paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}