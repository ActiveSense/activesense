using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Services;

public interface IResultParserService
{
    Task<IEnumerable<AnalysisResult>> ParseScript(string outputDirectory);
}

public class ResultParserService : IResultParserService
{
    public async Task<IEnumerable<AnalysisResult>> ParseScript(string outputDirectory)
    {
        var results = new List<AnalysisResult>();

        if (!Directory.Exists(outputDirectory))
        {
            return results;
        }

        var files = Directory.GetFiles(outputDirectory, "*.csv");

        foreach (var file in files)
        {
            try
            {
                var fileName = Path.GetFileName(file);
                var analysisType = fileName.StartsWith("Sleep_") ? AnalysisType.Sleep :
                    fileName.StartsWith("Activity_") ? AnalysisType.Activity : AnalysisType.Unknown;

                var result = new AnalysisResult
                {
                    FilePath = file,
                    FileName = fileName,
                    AnalysisType = analysisType,
                };
                
                results.Add(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        return results;
    }
}