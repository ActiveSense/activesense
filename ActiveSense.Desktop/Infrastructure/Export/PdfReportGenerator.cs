using System;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ActiveSense.Desktop.Infrastructure.Export;

public class PdfReportGenerator(
    IChartRenderer chartRenderer,
    IAnalysisSerializer serializer)
    : IPdfReportGenerator
{
    public async Task<bool> GeneratePdfReportAsync(IAnalysis analysis, string outputPath)
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
                        ConfigurePage(page);

                        // Content
                        page.Content()
                            .Column(column =>
                            {
                                // General Section
                                AddGeneralSection(column, chartProvider);

                                // Sleep Section
                                AddSleepSection(column, sleepAnalysis, chartProvider);

                                // Activity Section
                                AddActivitySection(column, activityAnalysis, chartProvider);

                                // Hidden serialization data
                                AddHiddenData(column, exportData);
                            });

                        // Footer
                        AddFooter(page);
                    });
                }).GeneratePdf(outputPath);
            });

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating PDF report: {ex.Message}");
        }
    }

    private void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(1.5f, Unit.Centimetre);
        page.DefaultTextStyle(x => x.FontSize(10));
    }

    private void AddHeaderSection(ColumnDescriptor column, IAnalysis analysis, string dateRangeText)
    {
        // Title
        column.Item().PaddingBottom(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(120);
                });

                table.Cell().Element(e =>
                    e.Text($"ActiveSense: {analysis.FileName}")
                      .FontSize(16)
                      .Bold());

                table.Cell().Element(e =>
                    e.AlignRight().Text(DateTime.Now.ToString("yyyy-MM-dd"))
                      .FontSize(9)
                      .FontColor(Colors.Grey.Medium));
            });

        // Date range
        column.Item().PaddingBottom(10)
            .Text(dateRangeText)
            .FontSize(10)
            .FontColor(Colors.Grey.Darken1);

        // Tags
        AddTags(column, analysis);

        // Section separator
        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
    }

    private void AddTags(ColumnDescriptor column, IAnalysis analysis)
    {
        if (analysis.Tags.Any())
        {
            column.Item().PaddingBottom(5)
                .Text(text =>
                {
                    text.Span("Tags: ").FontSize(9);
                    for (int i = 0; i < analysis.Tags.Count; i++)
                    {
                        var tag = analysis.Tags[i];
                        text.Element().Background(tag.Color)
                            .Padding(3)
                            .Text(tag.Name)
                            .FontColor(tag.TextColor)
                            .FontSize(8);

                        if (i < analysis.Tags.Count - 1)
                            text.Span(" ");
                    }
                });
        }
    }

    private void AddGeneralSection(ColumnDescriptor column, IChartDataProvider chartProvider)
    {
        // Section title
        column.Item().PaddingTop(15)
            .Text("Allgemeine Übersicht")
            .Bold()
            .FontSize(14);

        column.Item().PaddingTop(3)
            .Text("Beziehung zwischen täglicher Aktivität und Schlafqualität")
            .FontSize(9)
            .Italic();

        // Two column layout for charts
        column.Item().PaddingTop(10).Row(row =>
        {
            // Steps with Sleep Efficiency Chart
            row.RelativeItem().Column(col =>
            {
                col.Item().Element(e =>
                    e.Height(160)
                     .Image(chartRenderer.RenderStepsWithSleepEfficiencyChart(chartProvider))
                     .FitArea());

                col.Item().PaddingTop(3)
                    .Text("Schritte und Schlafeffizienz")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium)
                    .Italic()
                    .AlignCenter();
            });

            // Movement Pattern Pie Chart
            row.RelativeItem().Column(col =>
            {
                col.Item().Element(e =>
                    e.Height(160)
                     .Image(chartRenderer.RenderMovementPatternChart(chartProvider))
                     .FitArea());

                col.Item().PaddingTop(3)
                    .Text("24-Stunden Aktivitätsverteilung")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium)
                    .Italic()
                    .AlignCenter();
            });
        });

        // Section separator
        column.Item().PaddingTop(15)
            .LineHorizontal(1)
            .LineColor(Colors.Grey.Lighten2);
    }

    private void AddSleepSection(ColumnDescriptor column, ISleepAnalysis sleepAnalysis, IChartDataProvider chartProvider)
    {
        // Section title
        column.Item().PaddingTop(15)
            .Text("Schlafanalyse")
            .Bold()
            .FontSize(14);

        if (sleepAnalysis.SleepRecords.Count == 0)
        {
            column.Item().Text("Keine Schlafdaten verfügbar").Italic();
        }
        else
        {
            column.Item().PaddingTop(3)
                .Text("Analyse des Schlafverhaltens mit Fokus auf Schlafeffizienz")
                .FontSize(9)
                .Italic();

            // Sleep Data Summary Table
            AddSleepSummaryTable(column, sleepAnalysis);

            // Charts in two columns
            column.Item().PaddingTop(10).Row(row =>
            {
                // Sleep Distribution Pie Chart
                row.RelativeItem().Column(col =>
                {
                    col.Item().Element(e =>
                        e.Height(160)
                         .Image(chartRenderer.RenderSleepDistributionChart(chartProvider))
                         .FitArea());

                    col.Item().PaddingTop(3)
                        .Text("Verteilung Schlaf- und Wachzeit")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium)
                        .Italic()
                        .AlignCenter();
                });

                // Sleep with Efficiency Chart
                row.RelativeItem().Column(col =>
                {
                    col.Item().Element(e =>
                        e.Height(160)
                         .Image(chartRenderer.RenderSleepWithEfficiencyChart(chartProvider))
                         .FitArea());

                    col.Item().PaddingTop(3)
                        .Text("Schlafzeit (h) und Effizienz (%)")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium)
                        .Italic()
                        .AlignCenter();
                });
            });
        }

        // Section separator
        column.Item().PaddingTop(15)
            .LineHorizontal(1)
            .LineColor(Colors.Grey.Lighten2);
    }

    private void AddSleepSummaryTable(ColumnDescriptor column, ISleepAnalysis sleepAnalysis)
    {
        column.Item().PaddingTop(10)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(110);
                    columns.RelativeColumn();
                    columns.ConstantColumn(110);
                    columns.RelativeColumn();
                });

                // Row 1
                AddTableRow(table, "Durchschn. Schlafzeit", $"{sleepAnalysis.AverageSleepTime / 3600:F1} Std.");
                AddTableRow(table, "Schlafeffizienz", $"{sleepAnalysis.SleepEfficiency.Average():F1}%");

                // Row 2
                table.Cell().Element(e => e.Text("Durchschn. Wachzeit").Bold());
                table.Cell().Element(e => e.Text($"{sleepAnalysis.AverageWakeTime / 60:F0} Min."));
                table.Cell().Element(e => e.Text("Aktive Perioden").Bold());
                table.Cell().Element(e =>
                    e.Text($"{sleepAnalysis.SleepRecords.Average(r => double.Parse(r.NumActivePeriods)):F1}/Nacht"));
            });
    }

    private void AddActivitySection(ColumnDescriptor column, IActivityAnalysis activityAnalysis, IChartDataProvider chartProvider)
    {
        // Section title
        column.Item().PaddingTop(15)
            .Text("Aktivitätsanalyse")
            .Bold()
            .FontSize(14);

        if (activityAnalysis.ActivityRecords == null || !activityAnalysis.ActivityRecords.Any())
        {
            column.Item().Text("Keine Aktivitätsdaten verfügbar").Italic();
        }
        else
        {
            column.Item().PaddingTop(3)
                .Text("Analyse der täglichen Aktivität und Bewegungsmuster")
                .FontSize(9)
                .Italic();

            // Activity Data Summary Table
            AddActivitySummaryTable(column, activityAnalysis);

            // Charts in two columns
            column.Item().PaddingTop(10).Row(row =>
            {
                // Steps Chart
                row.RelativeItem().Column(col =>
                {
                    col.Item().Element(e =>
                        e.Height(160)
                         .Image(chartRenderer.RenderStepsChart(chartProvider))
                         .FitArea());

                    col.Item().PaddingTop(3)
                        .Text("Tägliche Schritte")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium)
                        .Italic()
                        .AlignCenter();
                });

                // Activity Distribution Chart
                row.RelativeItem().Column(col =>
                {
                    col.Item().Element(e =>
                        e.Height(160)
                         .Image(chartRenderer.RenderActivityDistributionChart(chartProvider))
                         .FitArea());

                    col.Item().PaddingTop(3)
                        .Text("Aktivitätsstufen pro Tag (h)")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium)
                        .Italic()
                        .AlignCenter();
                });
            });
        }
    }

    private void AddActivitySummaryTable(ColumnDescriptor column, IActivityAnalysis activityAnalysis)
    {
        column.Item().PaddingTop(10)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(110);
                    columns.RelativeColumn();
                    columns.ConstantColumn(110);
                    columns.RelativeColumn();
                });

                // Row 1
                AddTableRow(table, "Durchschn. Schritte", $"{activityAnalysis.StepsPerDay.Average():N0}");
                AddTableRow(table, "Sitzzeit", $"{activityAnalysis.AverageSedentaryTime / 3600:F1} Std./Tag");

                // Row 2
                table.Cell().Element(e => e.Text("Leichte Aktivität").Bold());
                table.Cell().Element(e => e.Text($"{activityAnalysis.AverageLightActivity / 60:F0} Min./Tag"));
                table.Cell().Element(e => e.Text("Moderate Aktivität").Bold());
                table.Cell().Element(e => e.Text($"{activityAnalysis.AverageModerateActivity / 60:F0} Min./Tag"));

                // Row 3
                table.Cell().Element(e => e.Text("Intensive Aktivität").Bold());
                table.Cell().Element(e => e.Text($"{activityAnalysis.AverageVigorousActivity / 60:F0} Min./Tag"));
                table.Cell().Element(e => e.Text(""));
                table.Cell().Element(e => e.Text(""));
            });
    }

    private void AddHiddenData(ColumnDescriptor column, string exportData)
    {
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

    private void AddFooter(PageDescriptor page)
    {
        page.Footer()
            .AlignCenter()
            .Text(text =>
            {
                text.Span("ActiveSense | ")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);
                text.Span("Seite ")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber()
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);
                text.Span(" von ")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);
                text.TotalPages()
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);
            });
    }

    private void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(e => e.Text(label).Bold());
        table.Cell().Element(e => e.Text(value));
    }
}