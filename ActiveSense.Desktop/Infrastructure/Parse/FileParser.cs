using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using CsvHelper;
using Serilog;

namespace ActiveSense.Desktop.Infrastructure.Parse;

public class FileParser(IHeaderAnalyzer headerAnalyzer, DateToWeekdayConverter dateConverter, ILogger logger)
    : IFileParser
{
    public async Task<IAnalysis> ParseCsvDirectoryAsync(string directory)
    {
        var analysis = new GeneActiveAnalysis(dateConverter)
        {
            FilePath = directory,
            FileName = Path.GetFileName(directory)
        };

        var csvFiles = Directory.GetFiles(directory, "*.csv");

        await Task.Run(() =>
        {
            foreach (var file in csvFiles)
                try
                {
                    logger.Information("Parsing CSV file: {File}", file);
                    ParseCsvFile(file, analysis);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error parsing CSV file: {File}", file);
                    logger.Information("Removing invalid CSV file: {File}", file);
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception deleteEx)
                    {
                        logger.Error(deleteEx, "Error deleting invalid CSV file: {File}", file);
                    }
                    throw new Exception($"Error parsing file {Path.GetFileName(file)}: {e.Message}", e);
                }
        });

        return analysis;
    }

    public AnalysisType DetermineAnalysisType(string[] headers)
    {
        if (headerAnalyzer.IsActivityCsv(headers)) return AnalysisType.Activity;
        if (headerAnalyzer.IsSleepCsv(headers)) return AnalysisType.Sleep;
        return AnalysisType.Unknown;
    }

    public bool ParseCsvFile(string filePath, IAnalysis analysis)
    {
        if (analysis is not (IActivityAnalysis activityAnalysis and ISleepAnalysis sleepAnalysis)) return false;

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        if (headers == null) return false;
        var analysisType = DetermineAnalysisType(headers);

        switch (analysisType)
        {
            case AnalysisType.Activity:
                activityAnalysis.SetActivityRecords(csv.GetRecords<ActivityRecord>().ToList());
                return true;

            case AnalysisType.Sleep:
                sleepAnalysis.SetSleepRecords(csv.GetRecords<SleepRecord>().ToList());
                return true;

            case AnalysisType.Unknown:

            default:
                return false;
        }
    }
}