using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using CsvHelper;

namespace ActiveSense.Desktop.Sensors;

public class GeneActiveResultParser : IResultParser
{
    public async Task<IEnumerable<Analysis>> ParseResultsAsync(string outputDirectory)
    {
        var analyses = new List<Analysis>();

        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine($"Directory {outputDirectory} does not exist.");
            return analyses;
        }

        var directories = Directory.GetDirectories(outputDirectory);

        foreach (var directory in directories)
        {
            Console.WriteLine("Processing directory: " + directory);
            var analysis = new Analysis
            {
                FilePath = directory,
                FileName = Path.GetFileName(directory)
            };

            var csvFiles = Directory.GetFiles(directory, "*.csv");

            foreach (var file in csvFiles)
            {
                Console.WriteLine("Parsing file: " + file);
                try
                {
                    using var reader = new StreamReader(file);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                    csv.Read();
                    csv.ReadHeader();
                    var headers = csv.HeaderRecord;

                    var analysisType = DetermineAnalysisType(headers);

                    if (analysisType == AnalysisType.Activity)
                    {
                        analysis.ActivityRecords = csv.GetRecords<ActivityRecord>().ToList();
                    }
                    else if (analysisType == AnalysisType.Sleep)
                    {
                        analysis.SleepRecords = csv.GetRecords<SleepRecord>().ToList();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error parsing file {file}: {e.Message}");
                    throw;
                }
            }

            analyses.Add(analysis);
        }

        return analyses;
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
}