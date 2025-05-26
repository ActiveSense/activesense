using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ExportTests;

[TestFixture]
public class GeneActiveExporterTests
{
    [SetUp]
    public void Setup()
    {
        _mockPdfReportGenerator = new Mock<IPdfReportGenerator>();
        _mockCsvExporter = new Mock<ICsvExporter>();
        _mockArchiveCreator = new Mock<IArchiveCreator>();
        _mockLogger = new Mock<ILogger>();

        _exporter = new GeneActiveExporter(
            _mockPdfReportGenerator.Object,
            _mockCsvExporter.Object,
            _mockArchiveCreator.Object,
            _mockLogger.Object);

        _dateConverter = new DateToWeekdayConverter();
        _mockAnalysis = new GeneActiveAnalysis(_dateConverter)
        {
            FileName = "TestAnalysis"
        };
    }

    private Mock<IPdfReportGenerator> _mockPdfReportGenerator;
    private Mock<ICsvExporter> _mockCsvExporter;
    private Mock<IArchiveCreator> _mockArchiveCreator;
    private GeneActiveExporter _exporter;
    private GeneActiveAnalysis _mockAnalysis;
    private DateToWeekdayConverter _dateConverter;
    private Mock<ILogger> _mockLogger;

    [Test]
    public async Task ExportAsync_WithoutRawData_CallsOnlyPdfGenerator()
    {
        // Arrange
        var outputPath = "test.pdf";
        _mockPdfReportGenerator.Setup(x => x.GeneratePdfReportAsync(_mockAnalysis, outputPath))
            .ReturnsAsync(true);

        // Act
        var result = await _exporter.ExportAsync(_mockAnalysis, outputPath);

        // Assert
        Assert.That(result, Is.True);
        _mockPdfReportGenerator.Verify(x => x.GeneratePdfReportAsync(_mockAnalysis, outputPath), Times.Once);
        _mockCsvExporter.Verify(x => x.ExportSleepRecords(It.IsAny<IEnumerable<SleepRecord>>()), Times.Never);
        _mockCsvExporter.Verify(x => x.ExportActivityRecords(It.IsAny<IEnumerable<ActivityRecord>>()), Times.Never);
        _mockArchiveCreator.Verify(
            x => x.CreateArchiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ExportAsync_WithRawData_GeneratesPdfAndZipArchive()
    {
        // Arrange
        var outputPath = "test.zip";
        var sleepCsv = "sleep data";
        var activityCsv = "activity data";

        _mockPdfReportGenerator.Setup(x => x.GeneratePdfReportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockCsvExporter.Setup(x => x.ExportSleepRecords(It.IsAny<IEnumerable<SleepRecord>>()))
            .Returns(sleepCsv);

        _mockCsvExporter.Setup(x => x.ExportActivityRecords(It.IsAny<IEnumerable<ActivityRecord>>()))
            .Returns(activityCsv);

        _mockArchiveCreator.Setup(x => x.CreateArchiveAsync(
                outputPath,
                It.IsAny<string>(),
                _mockAnalysis.FileName,
                sleepCsv,
                activityCsv))
            .ReturnsAsync(true);

        // Act
        var result = await _exporter.ExportAsync(_mockAnalysis, outputPath, true);

        // Assert
        Assert.That(result, Is.True);
        _mockPdfReportGenerator.Verify(x => x.GeneratePdfReportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>()),
            Times.Once);
        _mockCsvExporter.Verify(x => x.ExportSleepRecords(It.IsAny<IEnumerable<SleepRecord>>()), Times.Once);
        _mockCsvExporter.Verify(x => x.ExportActivityRecords(It.IsAny<IEnumerable<ActivityRecord>>()), Times.Once);
        _mockArchiveCreator.Verify(x => x.CreateArchiveAsync(
            outputPath,
            It.IsAny<string>(),
            _mockAnalysis.FileName,
            sleepCsv,
            activityCsv), Times.Once);
    }

    [Test]
    public async Task ExportAsync_WhenPdfExportFails_ReturnsFalse()
    {
        // Arrange
        var outputPath = "test.zip";

        _mockPdfReportGenerator.Setup(x => x.GeneratePdfReportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _exporter.ExportAsync(_mockAnalysis, outputPath, true);

        // Assert
        Assert.That(result, Is.False);
        _mockArchiveCreator.Verify(x => x.CreateArchiveAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ExportAsync_WhenAnalysisDoesNotImplementRequiredInterfaces_ReturnsFalse()
    {
        // Arrange
        var outputPath = "test.zip";
        var mockInvalidAnalysis = new Mock<IAnalysis>().Object;

        // Act
        var result = await _exporter.ExportAsync(mockInvalidAnalysis, outputPath, true);

        // Assert
        Assert.That(result, Is.False);
        _mockPdfReportGenerator.Verify(x => x.GeneratePdfReportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>()),
            Times.Never);
        _mockCsvExporter.Verify(x => x.ExportSleepRecords(It.IsAny<IEnumerable<SleepRecord>>()), Times.Never);
        _mockCsvExporter.Verify(x => x.ExportActivityRecords(It.IsAny<IEnumerable<ActivityRecord>>()), Times.Never);
        _mockArchiveCreator.Verify(x => x.CreateArchiveAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }
}