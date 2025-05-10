using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.FactoryTests;

[TestFixture]
public class SensorProcessorFactoryTests
{
    [SetUp]
    public void Setup()
    {
        // Create a mock GENEActiv processor
        _mockGeneActiveProcessor = new Mock<ISensorProcessor>();
        _mockGeneActiveProcessor.Setup(p => p.SupportedType).Returns(SensorTypes.GENEActiv);

        // Configure mock factory to return processor based on sensor type
        _mockProcessorFactory = new Mock<Func<SensorTypes, ISensorProcessor>>();
        _mockProcessorFactory.Setup(f => f(SensorTypes.GENEActiv)).Returns(_mockGeneActiveProcessor.Object);

        // Throw for unsupported sensor types
        _mockProcessorFactory.Setup(f => f(It.Is<SensorTypes>(t => t != SensorTypes.GENEActiv)))
            .Throws<InvalidOperationException>();

        // Create the factory with the mock function
        _sensorProcessorFactory = new SensorProcessorFactory(_mockProcessorFactory.Object);
    }

    private SensorProcessorFactory _sensorProcessorFactory;
    private Mock<Func<SensorTypes, ISensorProcessor>> _mockProcessorFactory;
    private Mock<ISensorProcessor> _mockGeneActiveProcessor;

    [Test]
    public void GetSensorProcessor_ForGeneActivType_ReturnsGeneActiveProcessor()
    {
        // Act
        var result = _sensorProcessorFactory.GetSensorProcessor(SensorTypes.GENEActiv);

        // Assert
        Assert.That(result, Is.SameAs(_mockGeneActiveProcessor.Object));
        Assert.That(result.SupportedType, Is.EqualTo(SensorTypes.GENEActiv));
        _mockProcessorFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Once);
    }

    [Test]
    public void GetSensorProcessor_ForUnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        // Create a value that doesn't exist in the enum
        var unsupportedType = (SensorTypes)999;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _sensorProcessorFactory.GetSensorProcessor(unsupportedType));
        _mockProcessorFactory.Verify(f => f(unsupportedType), Times.Once);
    }

    [Test]
    public void GetSensorProcessor_InvokesFactoryFunctionExactlyOnce()
    {
        // Act
        var result = _sensorProcessorFactory.GetSensorProcessor(SensorTypes.GENEActiv);

        // Request the same processor again
        var result2 = _sensorProcessorFactory.GetSensorProcessor(SensorTypes.GENEActiv);

        // Assert - factory function should be called each time
        _mockProcessorFactory.Verify(f => f(SensorTypes.GENEActiv), Times.Exactly(2));
    }

    [Test]
    public void GetSensorProcessor_ReturnsProcessorWithMatchingSupportedType()
    {
        // Arrange - set up a processor with mismatched supported type
        var mismatchedProcessor = new Mock<ISensorProcessor>();
        mismatchedProcessor.Setup(p => p.SupportedType).Returns(SensorTypes.GENEActiv);

        _mockProcessorFactory.Setup(f => f(SensorTypes.GENEActiv)).Returns(mismatchedProcessor.Object);

        // Act
        var result = _sensorProcessorFactory.GetSensorProcessor(SensorTypes.GENEActiv);

        // Assert - we should get the processor with a matching supported type
        Assert.That(result.SupportedType, Is.EqualTo(SensorTypes.GENEActiv));
    }
}