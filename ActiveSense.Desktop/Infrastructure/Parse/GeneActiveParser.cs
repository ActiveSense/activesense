using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Parse;

public class GeneActiveResultParser(IPdfParser pdfParser, IFileParser fileParser) : IResultParser
{
    private readonly ApplicationPageNames[] _analysisPages =
    [
        ApplicationPageNames.Allgemein,
        ApplicationPageNames.Aktivität,
        ApplicationPageNames.Schlaf,
    ];

    public ApplicationPageNames[] GetAnalysisPages()
    {
        return _analysisPages;
    }

    public async Task<IEnumerable<IAnalysis>> ParseResultsAsync(string outputDirectory)
    {
        var analyses = new List<IAnalysis>();

        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine($"Directory {outputDirectory} does not exist.");
            return analyses;
        }

        // Parse PDF files
        var pdfAnalyses = await pdfParser.ParsePdfFilesAsync(outputDirectory);
        analyses.AddRange(pdfAnalyses);

        // Parse CSV files in directories
        var csvAnalyses = await ParseCsvDirectoriesAsync(outputDirectory);
        analyses.AddRange(csvAnalyses);

        // Assign tags
        foreach (var analysis in analyses) AssignTags(analysis);

        return analyses;
    }

    private async Task<List<IAnalysis>> ParseCsvDirectoriesAsync(string outputDirectory)
    {
        var analyses = new List<IAnalysis>();
        var directories = Directory.GetDirectories(outputDirectory);

        foreach (var directory in directories)
        {
            var analysis = await fileParser.ParseCsvDirectoryAsync(directory);
            if (analysis != null) analyses.Add(analysis);
        }

        return analyses;
    }

    private void AssignTags(IAnalysis analysis)
    {
        if (analysis is ISleepAnalysis sleepAnalysis)
            if (sleepAnalysis.SleepRecords.Count != 0)
                analysis.AddTag("Schlafdaten", "#3277a8");

        if (analysis is IActivityAnalysis activityAnalysis)
            if (activityAnalysis.ActivityRecords.Count != 0)
                analysis.AddTag("Aktivitätsdaten", "#38a832");
    }
}