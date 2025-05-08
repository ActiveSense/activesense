using System;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Export.Interfaces;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Export.Implementations;

public class GeneActiveExporter(
    IPdfReportGenerator pdfReportGenerator,
    ICsvExporter csvExporter,
    IArchiveCreator archiveCreator)
    : IExporter
{
    public async Task<bool> ExportAsync(IAnalysis analysis, string outputPath, bool exportRawData = false)
    {
        if (exportRawData)
            return await ExportPdfAndCsvZipAsync(analysis, outputPath);

        return await pdfReportGenerator.GeneratePdfReportAsync(analysis, outputPath);
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
            Console.WriteLine($"Error exporting PDF and CSV data to zip: {ex.Message}");
            return false;
        }
    }
}