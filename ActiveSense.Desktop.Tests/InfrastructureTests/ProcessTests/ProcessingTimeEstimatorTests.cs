using System;
using System.Collections.Generic;
using ActiveSense.Desktop.Infrastructure.Process;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ProcessTests;

[TestFixture]
public class ProcessingTimeEstimatorTests
{
    [SetUp]
    public void Setup()
    {
        _estimator = new ProcessingTimeEstimator();
    }

    private ProcessingTimeEstimator _estimator;

    [Test]
    public void EstimateProcessingTime_WithBothAnalysisEnabled_IsNotZero()
    {
        // Arrange
        IList<ScriptArgument> arguments = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = true },
            new BoolArgument { Flag = "sleep", Value = true }
        };
        
        double totalSizeMB = 2400;

        // Act
        var result = _estimator.EstimateProcessingTime(totalSizeMB, arguments);

        // Assert
        Assert.That(result, Is.Not.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void EstimateProcessingTime_WithOnlyActivityEnabled_IsHalfTime()
    {
        // Arrange
        IList<ScriptArgument> argumentsBoth = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = true },
            new BoolArgument { Flag = "sleep", Value = true }
        };

        IList<ScriptArgument> argumentsActivityOnly = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = true },
            new BoolArgument { Flag = "sleep", Value = false }
        };

        double totalSizeMB = 2400;

        // Act
        var resultBoth = _estimator.EstimateProcessingTime(totalSizeMB, argumentsBoth);
        var resultActivityOnly = _estimator.EstimateProcessingTime(totalSizeMB, argumentsActivityOnly);

        // Assert
        Assert.That(resultActivityOnly.TotalSeconds, Is.EqualTo(resultBoth.TotalSeconds * 0.5).Within(0.1));
    }

    [Test]
    public void EstimateProcessingTime_WithOnlySleepEnabled_IsHalfTime()
    {
        // Arrange
        IList<ScriptArgument> argumentsBoth = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = true },
            new BoolArgument { Flag = "sleep", Value = true }
        };

        IList<ScriptArgument> argumentsSleepOnly = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = false },
            new BoolArgument { Flag = "sleep", Value = true }
        };

        double totalSizeMB = 2400;

        // Act
        var resultBoth = _estimator.EstimateProcessingTime(totalSizeMB, argumentsBoth);
        var resultSleepOnly = _estimator.EstimateProcessingTime(totalSizeMB, argumentsSleepOnly);

        // Assert
        Assert.That(resultSleepOnly.TotalSeconds, Is.EqualTo(resultBoth.TotalSeconds * 0.5).Within(0.1));
    }

    [Test]
    public void EstimateProcessingTime_WithSmallFileSize_IsZero()
    {
        // Arrange
        IList<ScriptArgument> arguments = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = true },
            new BoolArgument { Flag = "sleep", Value = true }
        };
        double totalSizeMB = 5; // Small file size

        // Act
        var result = _estimator.EstimateProcessingTime(totalSizeMB, arguments);

        // Assert
        Assert.That(result, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void EstimateProcessingTime_WithNegativeSize_IsZero()
    {
        // Arrange
        IList<ScriptArgument> arguments = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = true },
            new BoolArgument { Flag = "sleep", Value = true }
        };
        double totalSizeMB = -2400;

        // Act
        var result = _estimator.EstimateProcessingTime(totalSizeMB, arguments);

        // Assert
        Assert.That(result, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void EstimateProcessingTime_WithNoArguments_UsesFullTime()
    {
        // Arrange
        IList<ScriptArgument> arguments = null;
        double totalSizeMB = 2400;

        // Act
        var result = _estimator.EstimateProcessingTime(totalSizeMB, arguments);

        // Assert
        Assert.That(result, Is.Not.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void EstimateProcessingTime_WithBothAnalysisDisabled_IsMinimalTime()
    {
        // Arrange
        IList<ScriptArgument> argumentsBoth = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = true },
            new BoolArgument { Flag = "sleep", Value = true }
        };

        IList<ScriptArgument> argumentsNone = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "activity", Value = false },
            new BoolArgument { Flag = "sleep", Value = false }
        };

        double totalSizeMB = 2400;

        // Act
        var resultBoth = _estimator.EstimateProcessingTime(totalSizeMB, argumentsBoth);
        var resultNone = _estimator.EstimateProcessingTime(totalSizeMB, argumentsNone);

        // Assert
        Assert.That(resultNone.TotalSeconds, Is.EqualTo(resultBoth.TotalSeconds * 0.1).Within(0.1));
        Assert.That(resultNone, Is.Not.EqualTo(TimeSpan.Zero)); // Should still have some minimal time
    }
}