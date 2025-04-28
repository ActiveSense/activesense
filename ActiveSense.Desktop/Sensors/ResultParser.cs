using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using CsvHelper;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace ActiveSense.Desktop.Sensors;

public class GeneActiveResultParser(
    DateToWeekdayConverter dateToWeekdayConverter,
    AnalysisSerializer analysisSerializer)
    : IResultParser
{
    private readonly ApplicationPageNames[] _analysisPages =
    [
        ApplicationPageNames.Activity,
        ApplicationPageNames.Sleep,
        ApplicationPageNames.General
    ];

    public ApplicationPageNames[] GetAnalysisPages()
    {
        return _analysisPages;
    }

    public async Task<IEnumerable<Analysis>> ParseResultsAsync(string outputDirectory)
    {
        var analyses = new List<Analysis>();

        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine($"Directory {outputDirectory} does not exist.");
            return analyses;
        }

        // Parse PDF files
        var pdfAnalyses = await ParsePdfFilesAsync(outputDirectory);
        analyses.AddRange(pdfAnalyses);

        // Parse CSV files in directories
        var csvAnalyses = await ParseCsvDirectoriesAsync(outputDirectory);
        analyses.AddRange(csvAnalyses);

        return analyses;
    }

    public async Task<List<Analysis>> ParsePdfFilesAsync(string outputDirectory)
    {
        var analyses = new List<Analysis>();
        var pdfFiles = Directory.GetFiles(outputDirectory, "*.pdf");

        foreach (var file in pdfFiles)
        {
            var pdfText = ExtractTextFromPdf(file);
            var analysis = ExtractAnalysisFromPdfText(pdfText);
            
            if (analysis != null)
            {
                analysis.Exported = true;
                analyses.Add(analysis);
                Console.WriteLine("Extracted analysis from PDF: " + file);
            }
        }

        Console.WriteLine("Found " + pdfFiles.Length + " PDF files. In path: " + outputDirectory);
        return analyses;
    }

    public async Task<List<Analysis>> ParseCsvDirectoriesAsync(string outputDirectory)
    {
        var analyses = new List<Analysis>();
        var directories = Directory.GetDirectories(outputDirectory);

        foreach (var directory in directories)
        {
            Console.WriteLine("Processing directory: " + directory);
            var analysis = await ParseCsvDirectoryAsync(directory);
            
            if (analysis != null)
            {
                analyses.Add(analysis);
            }
        }

        return analyses;
    }

    public async Task<Analysis> ParseCsvDirectoryAsync(string directory)
    {
        var analysis = new Analysis(dateToWeekdayConverter)
        {
            FilePath = directory,
            FileName = Path.GetFileName(directory)
        };

        var csvFiles = Directory.GetFiles(directory, "*.csv");
        bool hasValidData = false;

        foreach (var file in csvFiles)
        {
            Console.WriteLine("Parsing file: " + file);
            try
            {
                if (ParseCsvFile(file, analysis))
                {
                    hasValidData = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error parsing file {file}: {e.Message}");
            }
        }

        return hasValidData ? analysis : null;
    }

    public bool ParseCsvFile(string filePath, Analysis analysis)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        var analysisType = DetermineAnalysisType(headers);

        if (analysisType == AnalysisType.Activity)
        {
            analysis.SetActivityRecords(csv.GetRecords<ActivityRecord>().ToList());
            return true;
        }

        if (analysisType == AnalysisType.Sleep)
        {
            analysis.SetSleepRecords(csv.GetRecords<SleepRecord>().ToList());
            return true;
        }

        return false;
    }

    private AnalysisType DetermineAnalysisType(string[] headers)
    {
        if (IsActivityCsv(headers)) return AnalysisType.Activity;
        if (IsSleepCsv(headers)) return AnalysisType.Sleep;
        return AnalysisType.Unknown;
    }

    private bool IsActivityCsv(string[] headers)
    {
        var activityHeaders = new[]
        {
            "Day.Number", "Steps", "Non_Wear", "Sleep",
            "Sedentary", "Light", "Moderate", "Vigorous"
        };

        return headers.Intersect(activityHeaders, StringComparer.OrdinalIgnoreCase).Count() >= 3;
    }

    private bool IsSleepCsv(string[] headers)
    {
        var sleepHeaders = new[]
        {
            "Night.Starting", "Sleep.Onset.Time", "Rise.Time",
            "Total.Sleep.Time", "Sleep.Efficiency"
        };

        return headers.Intersect(sleepHeaders, StringComparer.OrdinalIgnoreCase).Count() >= 3;
    }

    public Analysis ExtractAnalysisFromPdfText(string pdfText)
    {
        if (string.IsNullOrEmpty(pdfText))
            return null;

        try
        {
            const string startMarker = "ANALYSIS_DATA_BEGIN";
            const string endMarker = "ANALYSIS_DATA_END";

            var startIndex = pdfText.IndexOf(startMarker);
            if (startIndex == -1)
                return null;

            startIndex += startMarker.Length;

            var endIndex = pdfText.IndexOf(endMarker, startIndex);
            if (endIndex == -1)
                return null;

            var base64Content = pdfText.Substring(startIndex, endIndex - startIndex).Trim();

            base64Content = CleanBase64Content(base64Content);

            return analysisSerializer.ImportFromBase64(base64Content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting Analysis from PDF text: {ex.Message}");
            return null;
        }
    }

    private static string CleanBase64Content(string base64Content)
    {
        return Regex.Replace(base64Content, @"\s+", "");
    }

    private static string ExtractTextFromPdf(string filePath)
    {
        using var reader = new PdfReader(filePath);
        using var pdfDoc = new PdfDocument(reader);

        var text = new StringBuilder();
        for (var i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageText = PdfTextExtractor.GetTextFromPage(page);
            text.Append(pageText);
        }

        return text.ToString();
    }
}