using System;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using ActiveSense.Desktop.Infrastructure.Parse;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ImportTests;

[TestFixture]
public class PdfParserTests
{
    [SetUp]
    public void Setup()
    {
        _dateConverter = new DateToWeekdayConverter();
        _mockSerializer = new Mock<IAnalysisSerializer>();
        _mockLogger = new Mock<ILogger>();

        _realPdfParser = new PdfParser(_mockSerializer.Object, _dateConverter, _mockLogger.Object);
        _mockPdfParser = new Mock<PdfParser>(_mockSerializer.Object, _dateConverter, _mockLogger.Object);
        _mockPdfParser.CallBase = true;

        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }

    private Mock<IAnalysisSerializer> _mockSerializer;
    private DateToWeekdayConverter _dateConverter;
    private Mock<PdfParser> _mockPdfParser;
    private PdfParser _realPdfParser;
    private string _tempDir;
    private Mock<ILogger> _mockLogger;

    [Test]
    public void ExtractAnalysisFromPdfText_WithValidContent_ReturnsAnalysis()
    {
        // Arrange
        var mockAnalysis = new GeneActiveAnalysis(_dateConverter)
        {
            FileName = "TestAnalysis",
            FilePath = "/path/to/test"
        };

        _mockSerializer.Setup(x => x.ImportFromBase64("mockBase64Data"))
            .Returns(mockAnalysis);

        var pdfText = "Some PDF content ANALYSIS_DATA_BEGINmockBase64DataANALYSIS_DATA_END more PDF content";

        // Act
        var result = _realPdfParser.ExtractAnalysisFromPdfText(pdfText);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.SameAs(mockAnalysis));
        _mockSerializer.Verify(x => x.ImportFromBase64("mockBase64Data"), Times.Once);
    }

    [Test]
    public void ExtractAnalysisFromPdfText_WithMissingStartMarker_ThrowsException()
    {
        // Arrange
        var pdfText = "Some PDF content mockBase64Data ANALYSIS_DATA_END more PDF content";

        // Act & Assert
        Assert.Throws<Exception>(() => _realPdfParser.ExtractAnalysisFromPdfText(pdfText));
        _mockSerializer.Verify(x => x.ImportFromBase64(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ExtractAnalysisFromPdfText_WithMissingEndMarker_ThrowsException()
    {
        // Arrange
        var pdfText = "Some PDF content ANALYSIS_DATA_BEGIN mockBase64Data more PDF content";

        // Act & Assert
        Assert.Throws<Exception>(() => _realPdfParser.ExtractAnalysisFromPdfText(pdfText));
        _mockSerializer.Verify(x => x.ImportFromBase64(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ExtractAnalysisFromPdfText_WhenSerializerThrows_PropagatesException()
    {
        // Arrange
        _mockSerializer.Setup(x => x.ImportFromBase64(It.IsAny<string>()))
            .Throws(new Exception("Serializer error"));

        var pdfText = "Some PDF content ANALYSIS_DATA_BEGIN mockBase64Data ANALYSIS_DATA_END more PDF content";

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _realPdfParser.ExtractAnalysisFromPdfText(pdfText));
        Assert.That(ex.Message, Does.Contain("Error extracting Analysis"));
        _mockSerializer.Verify(x => x.ImportFromBase64(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void ExtractTextFromPdf_WithInvalidPdf_ThrowsException()
    {
        // Arrange
        var invalidPdfPath = Path.Combine(_tempDir, "invalid.pdf");
        File.WriteAllText(invalidPdfPath, "This is not a valid PDF file");

        // Act & Assert
        Assert.Throws<Exception>(() => _realPdfParser.ExtractTextFromPdf(invalidPdfPath));
    }

    [Test]
    public async Task ParsePdfFilesAsync_WithValidPdf_ReturnsAnalysisList()
    {
        // Arrange
        var mockAnalysis = new GeneActiveAnalysis(_dateConverter)
        {
            FileName = "TestAnalysis",
            FilePath = "/path/to/test"
        };

        // Create a test PDF file
        var pdfFilePath = Path.Combine(_tempDir, "test.pdf");
        File.WriteAllText(pdfFilePath, "%PDF-1.4\nMock PDF content");

        // Set up the mock to return controlled values
        var pdfText = "ANALYSIS_DATA_BEGIN mockBase64Data ANALYSIS_DATA_END";
        _mockPdfParser.Setup(x => x.ExtractTextFromPdf(pdfFilePath))
            .Returns(pdfText);
        _mockPdfParser.Setup(x => x.ExtractAnalysisFromPdfText(pdfText))
            .Returns(mockAnalysis);

        // Act
        var result = await _mockPdfParser.Object.ParsePdfFilesAsync(_tempDir);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0], Is.SameAs(mockAnalysis));
        Assert.That(result[0].FileName, Is.EqualTo("test"));
        Assert.That(result[0].Exported, Is.True);
    }

    [Test]
    public async Task ParsePdfFilesAsync_WithEmptyDirectory_ReturnsEmptyList()
    {
        // Act
        var result = await _realPdfParser.ParsePdfFilesAsync(_tempDir);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ParsePdfFilesAsync_WithNonExistingDirectory_ReturnsEmptyList()
    {
        // Arrange
        var nonExistingDir = Path.Combine(_tempDir, "non_existing");

        // Act
        var result = await _realPdfParser.ParsePdfFilesAsync(nonExistingDir);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public Task ParsePdfFilesAsync_WhenExtractionFails_HaltsProcessing()
    {
        // Arrange
        // Create files
        var validPdfPath = Path.Combine(_tempDir, "valid.pdf");
        var invalidPdfPath = Path.Combine(_tempDir, "invalid.pdf");

        File.WriteAllText(validPdfPath, "%PDF-1.4\nMock PDF content");
        File.WriteAllText(invalidPdfPath, "%PDF-1.4\nInvalid PDF content");

        var mockLogger = new Mock<ILogger>().Object;

        var mockAnalysis = new GeneActiveAnalysis(_dateConverter) { FileName = "valid" };

        // Set up the mock to succeed for valid file but throw for invalid file
        var validPdfText = "ANALYSIS_DATA_BEGIN mockBase64Data ANALYSIS_DATA_END";
        _mockPdfParser.Setup(x => x.ExtractTextFromPdf(validPdfPath))
            .Returns(validPdfText);
        _mockPdfParser.Setup(x => x.ExtractTextFromPdf(invalidPdfPath))
            .Throws(new Exception("Invalid PDF"));
        _mockPdfParser.Setup(x => x.ExtractAnalysisFromPdfText(validPdfText))
            .Returns(mockAnalysis);

        // Act & Assert
        // Since _mockPdfParser is already a mock, just use it directly
        Assert.ThrowsAsync<InvalidDataException>(() => _mockPdfParser.Object.ParsePdfFilesAsync(_tempDir));
        return Task.CompletedTask;
    }
}