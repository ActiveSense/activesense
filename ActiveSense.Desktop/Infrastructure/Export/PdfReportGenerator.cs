using System;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ActiveSense.Desktop.Infrastructure.Export;

public class PdfReportGenerator(
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
                                // Title and date
                                column.Item().PaddingBottom(5)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(120);
                                        });

                                        table.Cell().Element(e =>
                                            e.Text(analysis.FileName)
                                              .FontSize(16)
                                              .Bold());

                                        table.Cell().Element(e =>
                                            e.AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy"))
                                              .FontSize(9)
                                              .FontColor(Colors.Grey.Medium));
                                    });

                                AddTags(column, analysis);

                                AddGeneralSection(column, sleepAnalysis, activityAnalysis);

                                AddSleepSection(column, sleepAnalysis);

                                AddActivitySection(column, activityAnalysis);

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
            throw new Exception("Error generating PDF report: " + ex.Message);
        }
    }

    private void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(1.5f, Unit.Centimetre);
        page.DefaultTextStyle(x => x.FontSize(10));
    }

    private void AddTags(ColumnDescriptor column, IAnalysis analysis)
    {
        if (analysis.Tags.Any())
        {
            column.Item().PaddingBottom(5)
                .Text(text =>
                {
                    text.Span("Verfügbare Daten: ").FontSize(9);
                
                    var tagNames = analysis.Tags.Select(t => t.Name);
                    text.Span(string.Join(", ", tagNames))
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
        }
    }

    private void AddGeneralSection(ColumnDescriptor column, ISleepAnalysis sleepAnalysis, IActivityAnalysis activityAnalysis)
    {
        column.Item().PaddingTop(15)
            .Text("Zeitraum der Analyse")
            .Bold()
            .FontSize(14);

        if (activityAnalysis.ActivityRecords.Any())
        {
            column.Item().PaddingTop(5)
                .Text($"Aktivitätsdaten: {activityAnalysis.GetActivityDateRange()}")
                .FontSize(9)
                .FontColor(Colors.Grey.Darken1);
        }

        if (sleepAnalysis.SleepRecords.Any())
        {
            column.Item().PaddingTop(2)
                .Text($"Schlafdaten: {sleepAnalysis.GetSleepDateRange()}")
                .FontSize(9)
                .FontColor(Colors.Grey.Darken1);
        }

        column.Item().PaddingTop(15)
            .LineHorizontal(1)
            .LineColor(Colors.Grey.Lighten2);
    }

    private void AddSleepSection(ColumnDescriptor column, ISleepAnalysis sleepAnalysis)
    {
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
                .Text("Auswertung des Schlafverhaltens")
                .FontSize(9)
                .Italic();

            column.Item().PaddingTop(10)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(200);
                        columns.RelativeColumn();
                    });

                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Messwert").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Durchschnitt").Bold();

                    AddTableRow(table, "Tägliche Schlafzeit", $"{sleepAnalysis.AverageSleepTime / 3600:F1} Stunden");
                    AddTableRow(table, "Schlafeffizienz", $"{sleepAnalysis.SleepEfficiency.Average():F1}%");
                    AddTableRow(table, "Wachphasen pro Nacht", $"{sleepAnalysis.SleepRecords.Average(r => double.Parse(r.NumActivePeriods)):F1}");
                    AddTableRow(table, "Wachzeit pro Nacht", $"{sleepAnalysis.AverageWakeTime / 60:F0} Minuten");
                });
            
            column.Item().PaddingTop(5)
                .Text("Die Schlafeffizienz beschreibt das Verhältnis zwischen Schlafzeit und insgesamt im Bett verbrachter Zeit.")
                .FontSize(8)
                .FontColor(Colors.Grey.Medium)
                .Italic();
        }

        column.Item().PaddingTop(15)
            .LineHorizontal(1)
            .LineColor(Colors.Grey.Lighten2);
    }

    private void AddActivitySection(ColumnDescriptor column, IActivityAnalysis activityAnalysis)
    {
        column.Item().PaddingTop(15)
            .Text("Aktivitätsanalyse")
            .Bold()
            .FontSize(14);

        if (activityAnalysis.ActivityRecords.Count == 0)
        {
            column.Item().Text("Keine Aktivitätsdaten verfügbar").Italic();
        }
        else
        {
            column.Item().PaddingTop(3)
                .Text("Auswertung der täglichen Bewegung")
                .FontSize(9)
                .Italic();

            column.Item().PaddingTop(10)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(200);
                        columns.RelativeColumn();
                    });

                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Messwert").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Durchschnitt").Bold();

                    AddTableRow(table, "Schritte pro Tag", $"{activityAnalysis.StepsPerDay.Average():N0}");
                    AddTableRow(table, "Sitzzeit pro Tag", $"{activityAnalysis.AverageSedentaryTime / 3600:F1} Stunden");
                    AddTableRow(table, "Leichte Aktivität", $"{activityAnalysis.AverageLightActivity / 60:F0} Minuten pro Tag");
                    AddTableRow(table, "Mittlere Aktivität", $"{activityAnalysis.AverageModerateActivity / 60:F0} Minuten pro Tag");
                    AddTableRow(table, "Intensive Aktivität", $"{activityAnalysis.AverageVigorousActivity / 60:F0} Minuten pro Tag");
                });
            
            column.Item().PaddingTop(5)
                .Text("")
                .FontSize(8)
                .FontColor(Colors.Grey.Medium)
                .Italic();
        }
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
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
            .Text(label);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
            .Text(value);
    }
}