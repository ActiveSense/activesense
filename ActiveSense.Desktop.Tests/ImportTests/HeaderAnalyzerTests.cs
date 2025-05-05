using System;
using ActiveSense.Desktop.Import.Implementations;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ImportTests;

[TestFixture]
public class HeaderAnalyzerTests
{
    private HeaderAnalyzer _headerAnalyzer;

    [SetUp]
    public void Setup()
    {
        _headerAnalyzer = new HeaderAnalyzer();
    }

    [Test]
    public void IsActivityCsv_WithAllHeaders_ReturnsTrue()
    {
        // Arrange
        var headers = new[] { "Day.Number", "Steps", "Non_Wear", "Sleep", "Sedentary", "Light", "Moderate", "Vigorous" };

        // Act
        bool result = _headerAnalyzer.IsActivityCsv(headers);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsActivityCsv_WithMinimumRequiredHeaders_ReturnsTrue()
    {
        // Arrange
        var headers = new[] { "Day.Number", "Steps", "Sleep" };

        // Act
        bool result = _headerAnalyzer.IsActivityCsv(headers);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsActivityCsv_WithInsufficientHeaders_ReturnsFalse()
    {
        // Arrange
        var headers = new[] { "Day.Number", "Steps" };

        // Act
        bool result = _headerAnalyzer.IsActivityCsv(headers);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsActivityCsv_WithDifferentCasing_ReturnsTrue()
    {
        // Arrange
        var headers = new[] { "day.number", "steps", "non_wear", "sleep", "sedentary" };

        // Act
        bool result = _headerAnalyzer.IsActivityCsv(headers);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsActivityCsv_WithIrrelevantHeaders_ReturnsFalse()
    {
        // Arrange
        var headers = new[] { "Column1", "Column2", "Column3" };

        // Act
        bool result = _headerAnalyzer.IsActivityCsv(headers);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSleepCsv_WithAllHeaders_ReturnsTrue()
    {
        // Arrange
        var headers = new[] { "Night.Starting", "Sleep.Onset.Time", "Rise.Time", "Total.Sleep.Time", "Sleep.Efficiency" };

        // Act
        bool result = _headerAnalyzer.IsSleepCsv(headers);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSleepCsv_WithMinimumRequiredHeaders_ReturnsTrue()
    {
        // Arrange
        var headers = new[] { "Night.Starting", "Sleep.Onset.Time", "Sleep.Efficiency" };

        // Act
        bool result = _headerAnalyzer.IsSleepCsv(headers);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSleepCsv_WithInsufficientHeaders_ReturnsFalse()
    {
        // Arrange
        var headers = new[] { "Night.Starting", "Sleep.Onset.Time" };

        // Act
        bool result = _headerAnalyzer.IsSleepCsv(headers);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSleepCsv_WithDifferentCasing_ReturnsTrue()
    {
        // Arrange
        var headers = new[] { "night.starting", "sleep.onset.time", "rise.time", "sleep.efficiency" };

        // Act
        bool result = _headerAnalyzer.IsSleepCsv(headers);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSleepCsv_WithIrrelevantHeaders_ReturnsFalse()
    {
        // Arrange
        var headers = new[] { "Column1", "Column2", "Column3" };

        // Act
        bool result = _headerAnalyzer.IsSleepCsv(headers);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsActivityCsv_WithNullHeaders_ReturnsFalse()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            bool result = _headerAnalyzer.IsActivityCsv(null);
            Assert.That(result, Is.False);
        });
    }

    [Test]
    public void IsSleepCsv_WithNullHeaders_ReturnsFalse()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            bool result = _headerAnalyzer.IsSleepCsv(null);
            Assert.That(result, Is.False);
        });
    }

    [Test]
    public void IsActivityCsv_WithEmptyHeaders_ReturnsFalse()
    {
        // Arrange
        var headers = Array.Empty<string>();

        // Act
        bool result = _headerAnalyzer.IsActivityCsv(headers);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSleepCsv_WithEmptyHeaders_ReturnsFalse()
    {
        // Arrange
        var headers = Array.Empty<string>();

        // Act
        bool result = _headerAnalyzer.IsSleepCsv(headers);

        // Assert
        Assert.That(result, Is.False);
    }
}