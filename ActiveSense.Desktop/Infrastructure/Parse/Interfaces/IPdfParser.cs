using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

public interface IPdfParser
{
    Task<List<IAnalysis>> ParsePdfFilesAsync(string outputDirectory);
    IAnalysis ExtractAnalysisFromPdfText(string pdfText);
    string ExtractTextFromPdf(string filePath);
}