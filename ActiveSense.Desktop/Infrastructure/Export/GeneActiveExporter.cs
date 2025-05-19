using System;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Export;

public class GeneActiveExporter(
    IPdfReportGenerator pdfReportGenerator,
    ICsvExporter csvExporter,
    IArchiveCreator archiveCreator,
    Serilog.ILogger logger)
    : IExporter
{
    public async Task<bool> ExportAsync(IAnalysis analysis, string outputPath, bool exportRawData = false)
    {
        logger.Information("Exporting analysis to {outputPath}", outputPath);
        if (exportRawData)
            return await ExportPdfAndCsvZipAsync(analysis, outputPath);

        return await pdfReportGenerator.GeneratePdfReportAsync(analysis, outputPath);
    }

    private async Task<bool> ExportPdfAndCsvZipAsync(IAnalysis analysis, string outputPath)
    {
        if (analysis is not (IActivityAnalysis activityAnalysis and ISleepAnalysis sleepAnalysis))
        {
            return false;
        }

        try
        {
            var tempPdfPath = Path.GetTempFileName();

            try
            {
                var pdfSuccess = await pdfReportGenerator.GeneratePdfReportAsync(analysis, tempPdfPath);
                if (!pdfSuccess)
                    return false;

                var sleepCsv = csvExporter.ExportSleepRecords(sleepAnalysis.SleepRecords);
                var activityCsv = csvExporter.ExportActivityRecords(activityAnalysis.ActivityRecords);

                return await archiveCreator.CreateArchiveAsync(
                    outputPath, tempPdfPath, analysis.FileName, sleepCsv, activityCsv);
            }
            finally
            {
                if (File.Exists(tempPdfPath))
                    File.Delete(tempPdfPath);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error exporting PDF and CSV data to zip: {ex.Message}");
        }
    }
}