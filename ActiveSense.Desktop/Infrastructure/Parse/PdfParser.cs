using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace ActiveSense.Desktop.Infrastructure.Parse;

public class PdfParser(IAnalysisSerializer serializer, DateToWeekdayConverter dateConverter, Serilog.ILogger logger)
    : IPdfParser
{
    private readonly DateToWeekdayConverter _dateConverter = dateConverter;

    public async Task<List<IAnalysis>> ParsePdfFilesAsync(string outputDirectory)
    {
        var analyses = new List<IAnalysis>();
        string[] pdfFiles = [];

        try
        {
            pdfFiles = Directory.GetFiles(outputDirectory, "*.pdf");
        }
        catch
        {
            // ignored
        }

        await Task.Run(() =>
        {
            foreach (var file in pdfFiles)
            {
                try
                {
                    logger.Information("Parsing PDF file: {File}", file);
                    var pdfText = ExtractTextFromPdf(file);
                    var analysis = ExtractAnalysisFromPdfText(pdfText);

                    analysis.Exported = true;
                    analysis.FileName = Path.GetFileNameWithoutExtension(file);
                    analyses.Add(analysis);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error parsing PDF file: {File}", file);
                    throw new InvalidDataException($"Error parsing PDF file {file}: {e.Message}");
                }
            }
        });

        return analyses;
    }

    public virtual IAnalysis ExtractAnalysisFromPdfText(string pdfText)
    {
        try
        {
            const string startMarker = "ANALYSIS_DATA_BEGIN";
            const string endMarker = "ANALYSIS_DATA_END";

            var startIndex = pdfText.IndexOf(startMarker, StringComparison.Ordinal);
            if (startIndex < 0)
                throw new InvalidOperationException("Start marker not found in PDF text");

            startIndex += startMarker.Length;

            var endIndex = pdfText.IndexOf(endMarker, startIndex, StringComparison.Ordinal);
            if (endIndex < 0)
                throw new InvalidOperationException("End marker not found in PDF text");

            var base64Content = pdfText.Substring(startIndex, endIndex - startIndex).Trim();
            base64Content = CleanBase64Content(base64Content);

            return serializer.ImportFromBase64(base64Content);
        }
        catch (Exception ex)
        {
            throw new Exception("Error extracting Analysis from PDF text", ex);
        }
    }

    public virtual string ExtractTextFromPdf(string filePath)
    {
        try
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
        catch (Exception e)
        {
            throw new Exception($"Error extracting text from PDF file {filePath}: {e.Message}", e);
        }
    }

    private static string CleanBase64Content(string base64Content)
    {
        return Regex.Replace(base64Content, @"\s+", "");
    }
}