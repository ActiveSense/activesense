using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Import.Implementations;
using ActiveSense.Desktop.Import.Interfaces;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ImportTests;

[TestFixture]
public class ResultParserFactoryTests
{
    private Mock<Func<SensorTypes, IResultParser>> _mockParserFactory;
    private ResultParserFactory _factory;
    private Mock<IResultParser> _mockGeneActiveParser;
    
    [SetUp]
    public void Setup()
    {
        _mockGeneActiveParser = new Mock<IResultParser>();
        
        _mockParserFactory = new Mock<Func<SensorTypes, IResultParser>>();
        _mockParserFactory
            .Setup(f => f(SensorTypes.GENEActiv))
            .Returns(_mockGeneActiveParser.Object);
        
        _factory = new ResultParserFactory(_mockParserFactory.Object);
    }
    
    [Test]
    public void GetParser_WithGeneActiveType_ReturnsGeneActiveParser()
    {
        // Act
        var parser = _factory.GetParser(SensorTypes.GENEActiv);
        
        // Assert
        Assert.That(parser, Is.SameAs(_mockGeneActiveParser.Object));
        _mockParserFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Once);
    }
    
    [Test]
    public void GetParser_WithUnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockParserFactory
            .Setup(f => f(It.IsAny<SensorTypes>()))
            .Throws<InvalidOperationException>();
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _factory.GetParser((SensorTypes)999));
    }
}