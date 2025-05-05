using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using CsvHelper;
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
    public async Task<bool> ExportAsync(IAnalysis analysis, string outputPath, bool exportRawData = false)
    {
        if (exportRawData) return await ExportPdfAndCsvZipAsync(analysis, outputPath);

        return await ExportPdfReportAsync(analysis, outputPath);
    }

    private async Task<bool> ExportPdfReportAsync(IAnalysis analysis, string outputPath)
    {
        if (analysis is not (IActivityAnalysis activityAnalysis and ISleepAnalysis sleepAnalysis
            and IChartDataProvider chartProvider))
            return false;

        try
        {
            await Task.Run(() =>
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

                                if (sleepAnalysis.SleepRecords.Count == 0)
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
                                    
                                    column.Item().PaddingVertical(10)
                                        .Height(200)
                                        .Image(GenerateStepsChartImage(chartProvider));
                                }

                                // Activity Data Section
                                column.Item().PaddingTop(20)
                                    .Text("Aktivitätsdaten")
                                    .Bold()
                                    .FontSize(16);

                                if (activityAnalysis.ActivityRecords == null ||
                                    !activityAnalysis.ActivityRecords.Any())
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
                                        .Image(GenerateActivityDistributionChartImage(chartProvider));

                                    column.Item().ShowEntire().Column(innerColumn =>
                                    {
                                        innerColumn.Item().BorderBottom(1).BorderColor(Colors.Transparent)
                                            .PaddingTop(10).PaddingBottom(2)
                                            .Text("ANALYSIS_DATA_BEGIN")
                                            .FontSize(0.1f)
                                            .FontColor(Colors.Transparent);

                                        innerColumn.Item().Text(exportData)
                                            .FontSize(0.1f)
                                            .FontColor(Colors.Transparent);

                                        innerColumn.Item().PaddingTop(2)
                                            .Text("ANALYSIS_DATA_END")
                                            .FontSize(0.1f)
                                            .FontColor(Colors.Transparent);
                                    });
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
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PDF export failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ExportPdfAndCsvZipAsync(IAnalysis analysis, string outputPath)
    {
        if (analysis is not (IActivityAnalysis activityAnalysis and ISleepAnalysis sleepAnalysis))
        {
            Console.WriteLine("Analysis does not provide required capabilities for export");
            return false;
        }

        try
        {
            var tempPdfPath = Path.GetTempFileName();

            var pdfSuccess = await ExportPdfReportAsync(analysis, tempPdfPath);
            if (!pdfSuccess)
            {
                if (File.Exists(tempPdfPath))
                    File.Delete(tempPdfPath);
                return false;
            }

            var sleepCsv = SleepToCsv(sleepAnalysis.SleepRecords);
            var activityCsv = ActivityToCsv(activityAnalysis.ActivityRecords);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var pdfEntry = archive.CreateEntry($"{analysis.FileName}_report.pdf");
                await using (var entryStream = pdfEntry.Open())
                await using (var pdfStream = new FileStream(tempPdfPath, FileMode.Open, FileAccess.Read))
                {
                    await pdfStream.CopyToAsync(entryStream);
                }

                var sleepEntry = archive.CreateEntry($"{analysis.FileName}_sleep.csv");
                await using (var entryStream = sleepEntry.Open())
                await using (var writer = new StreamWriter(entryStream))
                {
                    await writer.WriteAsync(sleepCsv);
                }

                var activityEntry = archive.CreateEntry($"{analysis.FileName}_activity.csv");
                await using (var entryStream = activityEntry.Open())
                await using (var writer = new StreamWriter(entryStream))
                {
                    await writer.WriteAsync(activityCsv);
                }
            }

            zipStream.Seek(0, SeekOrigin.Begin);
            await using var fileStream = new FileStream(outputPath, FileMode.Create);
            await zipStream.CopyToAsync(fileStream);

            if (File.Exists(tempPdfPath))
                File.Delete(tempPdfPath);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting PDF and CSV data to zip: {ex.Message}");
            return false;
        }
    }

    private string SleepToCsv(IEnumerable<SleepRecord> sleepRecords)
    {
        using var stringWriter = new StringWriter();
        using var csv = new CsvWriter(stringWriter, CultureInfo.InvariantCulture);

        csv.WriteRecords(sleepRecords);
        csv.Flush();

        return stringWriter.ToString();
    }

    private string ActivityToCsv(IEnumerable<ActivityRecord> activityRecords)
    {
        using var stringWriter = new StringWriter();
        using var csv = new CsvWriter(stringWriter, CultureInfo.InvariantCulture);

        csv.WriteRecords(activityRecords);
        csv.Flush();

        return stringWriter.ToString();
    }

    private void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Text(label).Bold();
        table.Cell().Text(value);
    }

    #region Chart Generation
    
    private byte[] GeneratePieChartImage(IChartDataProvider analysis)
    {
        var dto = analysis.GetSleepDistributionChartData();
        var pieChartGenerator = new PieChartGenerator(dto, chartColors);
        var pieChartViewModel = pieChartGenerator.GenerateChart("Sleep Distribution", "");

        foreach (var series in pieChartViewModel.PieSeries.Cast<PieSeries<double>>())
        {
            series.DataLabelsSize = 14;
            series.DataLabelsPosition = PolarLabelsPosition.Middle;
            series.DataLabelsPaint = new SolidColorPaint(SKColors.White);
        }

        var pieChart = new SKPieChart
        {
            Series = pieChartViewModel.PieSeries,
            Width = 700,
            Height = 400,
            LegendPosition = LegendPosition.Right,
            TooltipPosition = TooltipPosition.Right
        };

        using var image = pieChart.GetImage();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private byte[] GenerateStepsChartImage(IChartDataProvider analysis)
    {
        var dto = analysis.GetStepsChartData();
        var barChartGenerator = new BarChartGenerator([dto], chartColors);
        var barChartViewModel = barChartGenerator.GenerateChart("Steps per Day", "");

        foreach (var series in barChartViewModel.Series.OfType<ColumnSeries<double>>())
        {
            series.DataLabelsSize = 12;
            series.DataLabelsPosition = DataLabelsPosition.Top;
            series.DataLabelsPaint = new SolidColorPaint(SKColors.Black);
        }

        var barChart = new SKCartesianChart
        {
            Series = barChartViewModel.Series,
            XAxes = barChartViewModel.XAxes,
            YAxes = barChartViewModel.YAxes,
            Width = 700,
            Height = 400,
            LegendPosition = LegendPosition.Bottom
        };

        using var image = barChart.GetImage();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private byte[] GenerateActivityDistributionChartImage(IChartDataProvider analysis)
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
    #endregion
}
