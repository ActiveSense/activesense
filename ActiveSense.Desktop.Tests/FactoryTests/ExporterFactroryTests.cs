using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.FactoryTests;

[TestFixture]
public class ExporterFactoryTests
{
    [SetUp]
    public void Setup()
    {
        // Create mock exporter
        _mockGeneActiveExporter = new Mock<IExporter>();

        // Configure mock factory
        _mockExporterFactory = new Mock<Func<SensorTypes, IExporter>>();
        _mockExporterFactory.Setup(f => f(SensorTypes.GENEActiv)).Returns(_mockGeneActiveExporter.Object);

        // Set up for unsupported types
        _mockExporterFactory.Setup(f => f(It.Is<SensorTypes>(t => t != SensorTypes.GENEActiv)))
            .Throws<InvalidOperationException>();

        // Create factory
        _exporterFactory = new ExporterFactory(_mockExporterFactory.Object);
    }

    private ExporterFactory _exporterFactory;
    private Mock<Func<SensorTypes, IExporter>> _mockExporterFactory;
    private Mock<IExporter> _mockGeneActiveExporter;

    [Test]
    public void GetExporter_ForGeneActivType_ReturnsGeneActiveExporter()
    {
        // Act
        var result = _exporterFactory.GetExporter(SensorTypes.GENEActiv);

        // Assert
        Assert.That(result, Is.SameAs(_mockGeneActiveExporter.Object));
        _mockExporterFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Once);
    }

    [Test]
    public void GetExporter_ForUnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        var unsupportedType = (SensorTypes)999;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _exporterFactory.GetExporter(unsupportedType));
        _mockExporterFactory.Verify(f => f(unsupportedType), Times.Once);
    }

    [Test]
    public void GetExporter_CalledMultipleTimes_InvokesFactoryEachTime()
    {
        // Act
        var result1 = _exporterFactory.GetExporter(SensorTypes.GENEActiv);
        var result2 = _exporterFactory.GetExporter(SensorTypes.GENEActiv);

        // Assert
        _mockExporterFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Exactly(2));
    }
}