using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Infrastructure.Export;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ExportTests;

[TestFixture]
public class ChartRendererTests
{
    private ChartRenderer _chartRenderer;
    private Mock<IChartDataProvider> _mockChartProvider;
    private ChartColors _chartColors;

    [SetUp]
    public void Setup()
    {
        _chartColors = new ChartColors();
        _chartRenderer = new ChartRenderer(_chartColors);
        _mockChartProvider = new Mock<IChartDataProvider>();

        // Setup mock chart data provider
        _mockChartProvider.Setup(x => x.GetSleepDistributionChartData())
            .Returns(new ChartDataDTO
            {
                Labels = new[] { "Sleep Time", "Wake Time" },
                Data = new[] { 7.5, 1.5 },
                Title = "Sleep Distribution"
            });

        _mockChartProvider.Setup(x => x.GetMovementPatternChartData())
            .Returns(new ChartDataDTO
            {
                Labels = new[] { "Activity", "Sleep", "Sedentary" },
                Data = new[] { 5.0, 7.5, 11.5 },
                Title = "Movement Pattern"
            });

        _mockChartProvider.Setup(x => x.GetStepsChartData())
            .Returns(new ChartDataDTO
            {
                Labels = new[] { "Monday", "Tuesday", "Wednesday" },
                Data = new[] { 8000.0, 10000.0, 7500.0 },
                Title = "Steps"
            });

        _mockChartProvider.Setup(x => x.GetSleepEfficiencyChartData())
            .Returns(new ChartDataDTO
            {
                Labels = new[] { "Monday", "Tuesday", "Wednesday" },
                Data = new[] { 80.0, 75.0, 82.0 },
                Title = "Sleep Efficiency"
            });

        _mockChartProvider.Setup(x => x.GetTotalSleepTimePerDayChartData())
            .Returns(new ChartDataDTO
            {
                Labels = new[] { "Monday", "Tuesday", "Wednesday" },
                Data = new[] { 7.5, 8.0, 6.5 },
                Title = "Total Sleep Time"
            });

        _mockChartProvider.Setup(x => x.GetActivityDistributionChartData())
            .Returns(new[]
            {
                new ChartDataDTO
                {
                    Labels = new[] { "Monday", "Tuesday", "Wednesday" },
                    Data = new[] { 2.0, 2.5, 3.0 },
                    Title = "Light Activity"
                },
                new ChartDataDTO
                {
                    Labels = new[] { "Monday", "Tuesday", "Wednesday" },
                    Data = new[] { 1.0, 1.5, 1.0 },
                    Title = "Moderate Activity"
                },
                new ChartDataDTO
                {
                    Labels = new[] { "Monday", "Tuesday", "Wednesday" },
                    Data = new[] { 0.5, 0.0, 0.5 },
                    Title = "Vigorous Activity"
                }
            });
    }

    [Test]
    public void RenderSleepDistributionChart_ReturnsByteArray()
    {
        // Act
        byte[] result = _chartRenderer.RenderSleepDistributionChart(_mockChartProvider.Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));

        // Verify the PNG header (first 8 bytes of a PNG file)
        Assert.That(result[0], Is.EqualTo(0x89)); // PNG signature
        Assert.That(result[1], Is.EqualTo(0x50)); // 'P'
        Assert.That(result[2], Is.EqualTo(0x4E)); // 'N'
        Assert.That(result[3], Is.EqualTo(0x47)); // 'G'
    }

    [Test]
    public void RenderMovementPatternChart_ReturnsByteArray()
    {
        // Act
        byte[] result = _chartRenderer.RenderMovementPatternChart(_mockChartProvider.Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));

        // Verify the PNG header
        Assert.That(result[0], Is.EqualTo(0x89)); // PNG signature
    }

    [Test]
    public void RenderStepsChart_ReturnsByteArray()
    {
        // Act
        byte[] result = _chartRenderer.RenderStepsChart(_mockChartProvider.Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));

        // Verify the PNG header
        Assert.That(result[0], Is.EqualTo(0x89)); // PNG signature
    }

    [Test]
    public void RenderSleepWithEfficiencyChart_ReturnsByteArray()
    {
        // Act
        byte[] result = _chartRenderer.RenderSleepWithEfficiencyChart(_mockChartProvider.Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));

        // Verify the PNG header
        Assert.That(result[0], Is.EqualTo(0x89)); // PNG signature
    }

    [Test]
    public void RenderStepsWithSleepEfficiencyChart_ReturnsByteArray()
    {
        // Act
        byte[] result = _chartRenderer.RenderStepsWithSleepEfficiencyChart(_mockChartProvider.Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));

        // Verify the PNG header
        Assert.That(result[0], Is.EqualTo(0x89)); // PNG signature
    }

    [Test]
    public void RenderActivityDistributionChart_ReturnsByteArray()
    {
        // Act
        byte[] result = _chartRenderer.RenderActivityDistributionChart(_mockChartProvider.Object);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));

        // Verify the PNG header
        Assert.That(result[0], Is.EqualTo(0x89)); // PNG signature
    }
}