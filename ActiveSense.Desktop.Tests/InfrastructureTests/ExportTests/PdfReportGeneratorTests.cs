using System;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ExportTests;

[TestFixture]
public class PdfReportGeneratorTests
{
    [SetUp]
    public void Setup()
    {
        _dateConverter = new DateToWeekdayConverter();

        // Create a test double for AnalysisSerializer
        _serializer = new TestAnalysisSerializer();

        _pdfGenerator = new PdfReportGenerator(
            _serializer);

        // Setup a valid analysis with some data
        _analysis = new GeneActiveAnalysis(_dateConverter)
        {
            FileName = "TestAnalysis",
            FilePath = "/path/to/test"
        };

        // Add some sleep records
        _analysis.SetSleepRecords(new[]
        {
            new SleepRecord
            {
                NightStarting = "2024-11-29",
                SleepOnsetTime = "21:25",
                RiseTime = "06:58",
                TotalElapsedBedTime = "34225",
                TotalSleepTime = "26676",
                TotalWakeTime = "7549",
                SleepEfficiency = "77.9",
                NumActivePeriods = "50",
                MedianActivityLength = "124"
            }
        });

        // Add some activity records
        _analysis.SetActivityRecords(new[]
        {
            new ActivityRecord
            {
                Day = "1",
                Steps = "3624",
                NonWear = "0",
                Sleep = "12994",
                Sedentary = "26283",
                Light = "14007",
                Moderate = "3286",
                Vigorous = "0"
            }
        });
    }

    private TestAnalysisSerializer _serializer;
    private PdfReportGenerator _pdfGenerator;
    private GeneActiveAnalysis _analysis;
    private DateToWeekdayConverter _dateConverter;

    // Minimal valid PNG file (1x1 transparent pixel)
    private static readonly byte[] ValidPngImageData =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
        0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00, 0x00, 0x00,
        0x0B, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0x60, 0x00, 0x02, 0x00,
        0x00, 0x05, 0x00, 0x01, 0xE2, 0x26, 0x05, 0x9B, 0x00, 0x00, 0x00, 0x00,
        0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    [Test]
    public async Task GeneratePdfReportAsync_WithValidAnalysis_ReturnsTrueAndCreatesPdf()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        try
        {
            // Act
            var result = await _pdfGenerator.GeneratePdfReportAsync(_analysis, tempFilePath);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(File.Exists(tempFilePath), Is.True);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
        }
    }

    [Test]
    public Task GeneratePdfReportAsync_WithInvalidOutput_Throws()
    {
        // Arrange
        var invalidPath = Path.Combine(Path.GetTempPath(), "invalid/path/that/does/not/exist.pdf");

        // Act
        var ex = Assert.ThrowsAsync<Exception>(async () =>
        {
            await _pdfGenerator.GeneratePdfReportAsync(_analysis, invalidPath);
        });

        if (ex != null) StringAssert.Contains("Error", ex.Message);
        return Task.CompletedTask;
    }

    [Test]
    public async Task GeneratePdfReportAsync_WithNonCompliantAnalysis_ReturnsFalse()
    {
        // Arrange
        var mockInvalidAnalysis = new Mock<IAnalysis>().Object;
        var tempFilePath = Path.GetTempFileName();

        try
        {
            // Act
            var result = await _pdfGenerator.GeneratePdfReportAsync(mockInvalidAnalysis, tempFilePath);

            // Assert
            Assert.That(result, Is.False);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
        }
    }

    [Test]
    public void GeneratePdfReportAsync_WhenSerializerThrows()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        var throwingSerializer = new ThrowingAnalysisSerializer();
        var pdfGeneratorWithThrowingSerializer = new PdfReportGenerator(
            throwingSerializer);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
        {
            await pdfGeneratorWithThrowingSerializer.GeneratePdfReportAsync(_analysis, tempFilePath);
        });

        StringAssert.Contains("Serializer error", ex.Message);

        // Cleanup
        if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
    }
}

// Test double for AnalysisSerializer
public class TestAnalysisSerializer : IAnalysisSerializer
{
    public string ExportToBase64(IAnalysis analysis)
    {
        return "mockBase64Data";
    }

    public IAnalysis ImportFromBase64(string base64)
    {
        var dateConverter = new DateToWeekdayConverter();
        return new GeneActiveAnalysis(dateConverter)
        {
            FileName = "MockImportedAnalysis"
        };
    }
}

public class ThrowingAnalysisSerializer : IAnalysisSerializer
{
    public string ExportToBase64(IAnalysis analysis)
    {
        throw new Exception("Serializer error");
    }

    public IAnalysis ImportFromBase64(string base64)
    {
        throw new Exception("Serializer error");
    }
}