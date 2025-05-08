using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Import.Interfaces;

public interface IPdfParser
{
    Task<List<IAnalysis>> ParsePdfFilesAsync(string outputDirectory);
    IAnalysis ExtractAnalysisFromPdfText(string pdfText);
    string ExtractTextFromPdf(string filePath);
}