using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Serilog;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ExportTests;

[TestFixture]
public class ExportIntegrationTests
{
    private IExporter _exporter;
    private GeneActiveAnalysis _analysis;
    private string _tempDir;

    [SetUp]
    public void Setup()
    {
        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        // Create all components directly instead of using mocks
        var dateConverter = new DateToWeekdayConverter();
        var chartColors = new ChartColors();
        var serializer = new AnalysisSerializer(dateConverter);
        var pdfGenerator = new PdfReportGenerator(serializer);
        var csvExporter = new CsvExporter();
        var archiveCreator = new ArchiveCreator();
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        

        // Create exporter with real components
        _exporter = new GeneActiveExporter(pdfGenerator, csvExporter, archiveCreator, logger);

        // Create and populate test analysis
        _analysis = new GeneActiveAnalysis(dateConverter)
        {
            FileName = "IntegrationTest",
            FilePath = Path.Combine(_tempDir, "test_path")
        };

        // Add some sleep records
        _analysis.SetSleepRecords([
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
            },
            new SleepRecord
            {
                NightStarting = "2024-11-30",
                SleepOnsetTime = "21:55",
                RiseTime = "08:03",
                TotalElapsedBedTime = "36393",
                TotalSleepTime = "26998",
                TotalWakeTime = "9395",
                SleepEfficiency = "74.2",
                NumActivePeriods = "67",
                MedianActivityLength = "84"
            }
        ]);

        // Add some activity records
        _analysis.SetActivityRecords(new[]
        {
            new ActivityRecord
            {
                Day = "2024-11-29",
                Steps = "3624",
                NonWear = "0",
                Sleep = "12994",
                Sedentary = "26283",
                Light = "14007",
                Moderate = "3286",
                Vigorous = "0"
            },
            new ActivityRecord
            {
                Day = "2024-11-29",
                Steps = "10217",
                NonWear = "0",
                Sleep = "26708",
                Sedentary = "29395",
                Light = "24346",
                Moderate = "4440",
                Vigorous = "2076"
            }
        });
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Test]
    public async Task ExportAsync_WithRawDataFalse_CreatesPdfFile()
    {
        // Arrange
        string pdfPath = Path.Combine(_tempDir, "output.pdf");

        // Act
        bool result = await _exporter.ExportAsync(_analysis, pdfPath, false);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(File.Exists(pdfPath), Is.True);

        // Verify the file is a valid PDF by checking for the PDF header
        using (var stream = File.OpenRead(pdfPath))
        {
            var buffer = new byte[5];
            int bytesRead = stream.Read(buffer, 0, 5);
            Assert.That(bytesRead, Is.EqualTo(5), "Failed to read expected number of bytes");
            Assert.That(Encoding.ASCII.GetString(buffer), Is.EqualTo("%PDF-"));
        }
    }

    [Test]
    public async Task ExportAsync_WithRawDataTrue_CreatesZipFile()
    {
        // Arrange
        string zipPath = Path.Combine(_tempDir, "output.zip");

        // Act
        bool result = await _exporter.ExportAsync(_analysis, zipPath, true);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(File.Exists(zipPath), Is.True);

        // Verify the file is a valid ZIP by checking for the ZIP header (PK..)
        using (var stream = File.OpenRead(zipPath))
        {
            var buffer = new byte[4];
            int bytesRead = stream.Read(buffer, 0, 4);
            Assert.That(bytesRead, Is.EqualTo(4), "Failed to read expected number of bytes");
            Assert.That(buffer[0], Is.EqualTo(0x50)); // P
            Assert.That(buffer[1], Is.EqualTo(0x4B)); // K
            Assert.That(buffer[2], Is.EqualTo(0x03)); // Control byte 1
            Assert.That(buffer[3], Is.EqualTo(0x04)); // Control byte 2
        }
    }

   [Test]
    public Task ExportAsync_WithInvalidOutputPath_ThrowsException()
    {
        // Arrange
        string invalidPath = Path.Combine(_tempDir, "nonexistent", "nested", "directory", "output.pdf");
    
        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
        {
            await _exporter.ExportAsync(_analysis, invalidPath, false);
        });
    
        StringAssert.Contains("Error generating PDF report", ex.Message);
        return Task.CompletedTask;
    }
}