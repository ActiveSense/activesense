using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.FactoryTests;

[TestFixture]
public class ResultParserFactoryTests
{
    [SetUp]
    public void Setup()
    {
        // Create mock parser
        _mockGeneActiveParser = new Mock<IResultParser>();

        // Configure mock factory
        _mockParserFactory = new Mock<Func<SensorTypes, IResultParser>>();
        _mockParserFactory.Setup(f => f(SensorTypes.GENEActiv)).Returns(_mockGeneActiveParser.Object);

        // Set up for unsupported types
        _mockParserFactory.Setup(f => f(It.Is<SensorTypes>(t => t != SensorTypes.GENEActiv)))
            .Throws<InvalidOperationException>();

        // Create factory
        _resultParserFactory = new ResultParserFactory(_mockParserFactory.Object);
    }

    private ResultParserFactory _resultParserFactory;
    private Mock<Func<SensorTypes, IResultParser>> _mockParserFactory;
    private Mock<IResultParser> _mockGeneActiveParser;

    [Test]
    public void GetParser_ForGeneActivType_ReturnsGeneActiveParser()
    {
        // Act
        var result = _resultParserFactory.GetParser(SensorTypes.GENEActiv);

        // Assert
        Assert.That(result, Is.SameAs(_mockGeneActiveParser.Object));
        _mockParserFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Once);
    }

    [Test]
    public void GetParser_ForUnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        var unsupportedType = (SensorTypes)999;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _resultParserFactory.GetParser(unsupportedType));
        _mockParserFactory.Verify(f => f(unsupportedType), Times.Once);
    }

    [Test]
    public void GetParser_CalledMultipleTimes_InvokesFactoryEachTime()
    {
        // Act
        var result1 = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
        var result2 = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
        var result3 = _resultParserFactory.GetParser(SensorTypes.GENEActiv);

        // Assert
        _mockParserFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Exactly(3));
    }
}