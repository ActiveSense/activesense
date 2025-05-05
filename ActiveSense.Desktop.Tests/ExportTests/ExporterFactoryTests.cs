using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Export.Interfaces;
using ActiveSense.Desktop.Factories;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ExportTests;

[TestFixture]
public class ExporterFactoryTests
{
    private Mock<Func<SensorTypes, IExporter>> _mockExporterFactory;
    private ExporterFactory _factory;
    private Mock<IExporter> _mockGeneActiveExporter;
    
    [SetUp]
    public void Setup()
    {
        _mockGeneActiveExporter = new Mock<IExporter>();
        
        _mockExporterFactory = new Mock<Func<SensorTypes, IExporter>>();
        _mockExporterFactory
            .Setup(f => f(SensorTypes.GENEActiv))
            .Returns(_mockGeneActiveExporter.Object);
        
        _factory = new ExporterFactory(_mockExporterFactory.Object);
    }
    
    [Test]
    public void GetExporter_WithGeneActiveType_ReturnsGeneActiveExporter()
    {
        // Act
        var exporter = _factory.GetExporter(SensorTypes.GENEActiv);
        
        // Assert
        Assert.That(exporter, Is.SameAs(_mockGeneActiveExporter.Object));
        _mockExporterFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Once);
    }
    
    [Test]
    public void GetExporter_WithUnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockExporterFactory
            .Setup(f => f(It.IsAny<SensorTypes>()))
            .Throws<InvalidOperationException>();
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _factory.GetExporter((SensorTypes)999));
    }
}