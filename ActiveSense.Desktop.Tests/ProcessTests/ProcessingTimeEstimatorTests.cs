using System;
using ActiveSense.Desktop.Infrastructure.Process;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ProcessTests;

[TestFixture]
public class ProcessingTimeEstimatorTests
{
    private ProcessingTimeEstimator _estimator;

    [SetUp]
    public void Setup()
    {
        _estimator = new ProcessingTimeEstimator();
    }

    [Test]
    public void EstimateProcessingTime_Is_Not_Zero()
    {
        double totalSizeMB = 2400;

        var result = _estimator.EstimateProcessingTime(totalSizeMB);
        
        Assert.That(result, Is.Not.EqualTo(TimeSpan.Zero));
    }
}