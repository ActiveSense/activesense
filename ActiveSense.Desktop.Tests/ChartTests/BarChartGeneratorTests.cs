using System;
using System.Linq;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ChartTests;

[TestFixture]
public class BarChartGeneratorTests
{
    private BarChartGenerator _generator;
    private ChartColors _chartColors;

    [SetUp]
    public void Setup()
    {
        _chartColors = new ChartColors();
    }

    #region NormalizeChartData Tests

    [Test]
    public void NormalizeChartData_WithMatchingLabels_ReturnsSameValues()
    {
        var data = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 10.0, 20.0, 30.0 },
            Title = "Test Data"
        };

        var allLabels = new[] { "Monday", "Tuesday", "Wednesday" };

        _generator = new BarChartGenerator(new[] { data }, _chartColors);

        var result = _generator.NormalizeChartData(data, allLabels);

        Assert.That(result.Length, Is.EqualTo(3), "Result should have 3 elements");
        Assert.That(result[0], Is.EqualTo(10.0).Within(0.001), "First element should be 10.0");
        Assert.That(result[1], Is.EqualTo(20.0).Within(0.001), "Second element should be 20.0");
        Assert.That(result[2], Is.EqualTo(30.0).Within(0.001), "Third element should be 30.0");
    }

    [Test]
    public void NormalizeChartData_WithAdditionalLabels_FillsWithZeros()
    {
        var data = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Wednesday" },
            Data = new[] { 10.0, 30.0 },
            Title = "Test Data"
        };

        var allLabels = new[] { "Monday", "Tuesday", "Wednesday", "Thursday" };

        _generator = new BarChartGenerator(new[] { data }, _chartColors);

        var result = _generator.NormalizeChartData(data, allLabels);

        Assert.That(result.Length, Is.EqualTo(4), "Result should have 4 elements");
        Assert.That(result[0], Is.EqualTo(10.0).Within(0.001), "Monday value should be 10.0");
        Assert.That(result[1], Is.EqualTo(0.0).Within(0.001), "Tuesday value should be 0.0");
        Assert.That(result[2], Is.EqualTo(30.0).Within(0.001), "Wednesday value should be 30.0");
        Assert.That(result[3], Is.EqualTo(0.0).Within(0.001), "Thursday value should be 0.0");
    }

    [Test]
    public void NormalizeChartData_WithDifferentOrderLabels_MapsCorrectly()
    {
        var data = new ChartDataDTO
        {
            Labels = new[] { "Wednesday", "Monday" },
            Data = new[] { 30.0, 10.0 },
            Title = "Test Data"
        };

        var allLabels = new[] { "Monday", "Tuesday", "Wednesday" };

        _generator = new BarChartGenerator(new[] { data }, _chartColors);

        var result = _generator.NormalizeChartData(data, allLabels);

        Assert.That(result.Length, Is.EqualTo(3), "Result should have 3 elements");
        Assert.That(result[0], Is.EqualTo(10.0).Within(0.001), "Monday value should be 10.0");
        Assert.That(result[1], Is.EqualTo(0.0).Within(0.001), "Tuesday value should be 0.0");
        Assert.That(result[2], Is.EqualTo(30.0).Within(0.001), "Wednesday value should be 30.0");
    }

    [Test]
    public void NormalizeChartData_WithEmptyInputs_ReturnsAllZeros()
    {
        var data = new ChartDataDTO
        {
            Labels = Array.Empty<string>(),
            Data = Array.Empty<double>(),
            Title = "Empty Data"
        };

        var allLabels = new[] { "Monday", "Tuesday", "Wednesday" };

        _generator = new BarChartGenerator(new[] { data }, _chartColors);

        var result = _generator.NormalizeChartData(data, allLabels);

        Assert.That(result.Length, Is.EqualTo(3), "Result should have 3 elements");
        Assert.That(result[0], Is.EqualTo(0.0).Within(0.001), "First element should be 0.0");
        Assert.That(result[1], Is.EqualTo(0.0).Within(0.001), "Second element should be 0.0");
        Assert.That(result[2], Is.EqualTo(0.0).Within(0.001), "Third element should be 0.0");
    }

    [Test]
    public void NormalizeChartData_WithFewerDataThanLabels_HandlesCorrectly()
    {
        var data = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 10.0, 20.0 },
            Title = "Test Data"
        };

        var allLabels = new[] { "Monday", "Tuesday", "Wednesday", "Thursday" };

        _generator = new BarChartGenerator(new[] { data }, _chartColors);

        var result = _generator.NormalizeChartData(data, allLabels);

        Assert.That(result.Length, Is.EqualTo(4), "Result should have 4 elements");
        Assert.That(result[0], Is.EqualTo(10.0).Within(0.001), "Monday value should be 10.0");
        Assert.That(result[1], Is.EqualTo(20.0).Within(0.001), "Tuesday value should be 20.0");
        Assert.That(result[2], Is.EqualTo(0.0).Within(0.001), "Wednesday value should be 0.0");
        Assert.That(result[3], Is.EqualTo(0.0).Within(0.001), "Thursday value should be 0.0");
    }

    #endregion

    #region GenerateChart Tests

    [Test]
    public void GenerateChart_WithNoData_ReturnsEmptyChart()
    {
        _generator = new BarChartGenerator(Array.Empty<ChartDataDTO>(), _chartColors);

        var result = _generator.GenerateChart("Empty Chart", "Test Description");

        Assert.That(result, Is.Not.Null, "Should return a non-null chart");
        Assert.That(result.Series, Is.Empty, "Series should be empty");
        Assert.That(result.XAxes.Length, Is.EqualTo(1), "Should have one X axis");
        Assert.That(result.XAxes[0].Labels.Count, Is.EqualTo(1), "X axis should have one label");
        Assert.That(result.XAxes[0].Labels[0], Is.EqualTo("No Data"), "X axis label should be 'No Data'");
    }

    [Test]
    public void GenerateChart_WithSingleSeries_GeneratesCorrectly()
    {
        var data = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 10.0, 20.0, 30.0 },
            Title = "Test Series"
        };

        _generator = new BarChartGenerator(new[] { data }, _chartColors);

        var result = _generator.GenerateChart("Test Chart", "Test Description");

        Assert.That(result, Is.Not.Null, "Should return a non-null chart");
        Assert.That(result.Title, Is.EqualTo("Test Chart"), "Chart title should match");
        Assert.That(result.Description, Is.EqualTo("Test Description"), "Chart description should match");

        // There should be 2 series: the bar series and the mean line
        Assert.That(result.Series.Length, Is.EqualTo(2), "Should have 2 series (bar + mean line)");

        // Check the first series is a column series
        Assert.That(result.Series[0], Is.TypeOf<ColumnSeries<double>>(), "First series should be a column series");
        var barSeries = (ColumnSeries<double>)result.Series[0];
        Assert.That(barSeries.Values.Count(), Is.EqualTo(3), "Bar series should have 3 data points");
        Assert.That(barSeries.Name, Is.EqualTo("Test Series"), "Bar series name should match");
        Assert.That(barSeries.ScalesYAt, Is.EqualTo(0), "Bar series should scale on first Y axis");

        // Check the mean line
        Assert.That(result.Series[1], Is.TypeOf<LineSeries<double>>(), "Second series should be a line series");
        var lineSeries = (LineSeries<double>)result.Series[1];
        Assert.That(lineSeries.Values.Count(), Is.EqualTo(3), "Line series should have 3 data points");
        Assert.That(lineSeries.Name, Is.EqualTo("Durchschnitt"), "Line series name should be 'Durchschnitt'");
        Assert.That(lineSeries.ScalesYAt, Is.EqualTo(0), "Mean line should scale on first Y axis");

        // Check mean value is correct (average of 10, 20, 30 = 20)
        var meanValue = lineSeries.Values.First();
        Assert.That(meanValue, Is.EqualTo(20.0).Within(0.001), "Mean value should be 20.0");

        // Check X axis
        Assert.That(result.XAxes.Length, Is.EqualTo(1), "Should have one X axis");
        Assert.That(result.XAxes[0].Labels.Count, Is.EqualTo(3), "X axis should have 3 labels");
        Assert.That(result.XAxes[0].Labels, Is.EquivalentTo(new[] { "Monday", "Tuesday", "Wednesday" }),
            "X axis labels should match input labels");

        // Check Y axes
        Assert.That(result.YAxes.Length, Is.EqualTo(1), "Should have one Y axis for bar data only");
        Assert.That(result.YAxes[0].Position, Is.EqualTo(AxisPosition.Start), "Y axis should be at start position (left)");
    }

    [Test]
    public void GenerateChart_WithMultipleSeries_CombinesLabelsCorrectly()
    {
        var data1 = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Wednesday" },
            Data = new[] { 10.0, 30.0 },
            Title = "Series 1"
        };

        var data2 = new ChartDataDTO
        {
            Labels = new[] { "Tuesday", "Thursday" },
            Data = new[] { 20.0, 40.0 },
            Title = "Series 2"
        };

        _generator = new BarChartGenerator(new[] { data1, data2 }, _chartColors);

        var result = _generator.GenerateChart("Combined Chart", "Test Description");

        Assert.That(result, Is.Not.Null, "Should return a non-null chart");

        // There should be 3 series: two bar series and the mean line
        Assert.That(result.Series.Length, Is.EqualTo(3), "Should have 3 series (2 bars + mean line)");

        // Check X axis has all labels combined
        Assert.That(result.XAxes.Length, Is.EqualTo(1), "Should have one X axis");
        Assert.That(result.XAxes[0].Labels.Count, Is.EqualTo(4), "X axis should have 4 labels");
        Assert.That(result.XAxes[0].Labels, Is.EquivalentTo(new[] { "Monday", "Wednesday", "Tuesday", "Thursday" }),
            "X axis labels should combine all input labels");

        // Check both bar series have same number of data points as combined labels
        var barSeries1 = (ColumnSeries<double>)result.Series[0];
        var barSeries2 = (ColumnSeries<double>)result.Series[1];
        Assert.That(barSeries1.Values.Count(), Is.EqualTo(4), "First bar series should have 4 data points");
        Assert.That(barSeries2.Values.Count(), Is.EqualTo(4), "Second bar series should have 4 data points");
        Assert.That(barSeries1.ScalesYAt, Is.EqualTo(0), "First bar series should scale on first Y axis");
        Assert.That(barSeries2.ScalesYAt, Is.EqualTo(0), "Second bar series should scale on first Y axis");

        // Check mean value is correct (average of 10, 30, 20, 40 = 25)
        var lineSeries = (LineSeries<double>)result.Series[2];
        var meanValue = lineSeries.Values.First();
        Assert.That(meanValue, Is.EqualTo(25.0).Within(0.001), "Mean value should be 25.0");
    }

    [Test]
    public void GenerateChart_WithLineData_DoesNotIncludeMeanLine()
    {
        var barData = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 10.0, 20.0, 30.0 },
            Title = "Bar Series"
        };

        var lineData = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 15.0, 25.0, 35.0 },
            Title = "Line Series"
        };

        _generator = new BarChartGenerator(new[] { barData }, _chartColors, new[] { lineData });

        var result = _generator.GenerateChart("Combined Chart", "Test Description");

        Assert.That(result, Is.Not.Null, "Should return a non-null chart");

        // There should be 2 series: bar series and line series (NO mean line)
        Assert.That(result.Series.Length, Is.EqualTo(2), "Should have 2 series (bar + line, no mean)");

        // Check the first series is a bar series
        Assert.That(result.Series[0], Is.TypeOf<ColumnSeries<double>>(), "First series should be a column series");
        var barSeries = (ColumnSeries<double>)result.Series[0];
        Assert.That(barSeries.Name, Is.EqualTo("Bar Series"), "Bar series name should match");
        Assert.That(barSeries.ScalesYAt, Is.EqualTo(0), "Bar series should scale on first Y axis");

        // Check the second series is the line series (not mean line)
        Assert.That(result.Series[1], Is.TypeOf<LineSeries<double>>(), "Second series should be a line series");
        var lineSeries = (LineSeries<double>)result.Series[1];
        Assert.That(lineSeries.Name, Is.EqualTo("Line Series"), "Line series name should match");
        Assert.That(lineSeries.ScalesYAt, Is.EqualTo(1), "Line series should scale on second Y axis");

        // Ensure no series is named "Durchschnitt" (mean)
        foreach (var series in result.Series)
        {
            Assert.That(series.Name, Is.Not.EqualTo("Durchschnitt"), "No series should be named 'Durchschnitt'");
        }

        // The line data should be mapped correctly
        var lineValues = lineSeries.Values.ToArray();
        Assert.That(lineValues.Length, Is.EqualTo(3), "Line series should have 3 data points");
        Assert.That(lineValues[0], Is.EqualTo(15.0).Within(0.001), "First line value should be 15.0");
        Assert.That(lineValues[1], Is.EqualTo(25.0).Within(0.001), "Second line value should be 25.0");
        Assert.That(lineValues[2], Is.EqualTo(35.0).Within(0.001), "Third line value should be 35.0");

        // Check Y axes - should have two for mixed bar and line
        Assert.That(result.YAxes.Length, Is.EqualTo(2), "Should have two Y axes for bar and line data");
        Assert.That(result.YAxes[0].Position, Is.EqualTo(AxisPosition.Start), "First Y axis should be at start position (left)");
        Assert.That(result.YAxes[1].Position, Is.EqualTo(AxisPosition.End), "Second Y axis should be at end position (right)");
    }

    [Test]
    public void GenerateChart_WithOnlyLineData_UsesFirstAxis()
    {
        var lineData = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 15.0, 25.0, 35.0 },
            Title = "Line Series"
        };

        _generator = new BarChartGenerator(null, _chartColors, new[] { lineData });

        var result = _generator.GenerateChart("Line Only Chart", "Test Description");

        Assert.That(result, Is.Not.Null, "Should return a non-null chart");

        // There should be just 1 series (the line)
        Assert.That(result.Series.Length, Is.EqualTo(1), "Should have 1 series (line only)");

        // Check the first series is a line series
        Assert.That(result.Series[0], Is.TypeOf<LineSeries<double>>(), "First series should be a line series");
        var lineSeries = (LineSeries<double>)result.Series[0];
        Assert.That(lineSeries.Name, Is.EqualTo("Line Series"), "Line series name should match");
        Assert.That(lineSeries.ScalesYAt, Is.EqualTo(0), "Line series should scale on first Y axis for line-only chart");

        // Check Y axes - should have only one for line-only data
        Assert.That(result.YAxes.Length, Is.EqualTo(1), "Should have one Y axis for line-only data");
        Assert.That(result.YAxes[0].Position, Is.EqualTo(AxisPosition.Start), "Y axis should be at start position (left)");
    }

    [Test]
    public void GenerateChart_WithMultipleLineData_UsesCorrectColors()
    {
        var lineData1 = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 15.0, 25.0, 35.0 },
            Title = "Line Series 1"
        };

        var lineData2 = new ChartDataDTO
        {
            Labels = new[] { "Monday", "Tuesday", "Wednesday" },
            Data = new[] { 5.0, 10.0, 15.0 },
            Title = "Line Series 2"
        };

        _generator = new BarChartGenerator(null, _chartColors, new[] { lineData1, lineData2 });

        var result = _generator.GenerateChart("Multiple Lines Chart", "Test Description");

        Assert.That(result, Is.Not.Null, "Should return a non-null chart");

        // There should be 2 series (the lines)
        Assert.That(result.Series.Length, Is.EqualTo(2), "Should have 2 series (2 lines)");

        // Check both series are line series
        Assert.That(result.Series[0], Is.TypeOf<LineSeries<double>>(), "First series should be a line series");
        Assert.That(result.Series[1], Is.TypeOf<LineSeries<double>>(), "Second series should be a line series");

        var lineSeries1 = (LineSeries<double>)result.Series[0];
        var lineSeries2 = (LineSeries<double>)result.Series[1];

        Assert.That(lineSeries1.Name, Is.EqualTo("Line Series 1"), "First line series name should match");
        Assert.That(lineSeries2.Name, Is.EqualTo("Line Series 2"), "Second line series name should match");

        // Both should scale on the same axis
        Assert.That(lineSeries1.ScalesYAt, Is.EqualTo(0), "First line series should scale on first Y axis");
        Assert.That(lineSeries2.ScalesYAt, Is.EqualTo(0), "Second line series should scale on first Y axis");

        // Check they have different colors (strokes)
        // We can't check exact colors easily, but we can verify the strokes are different
        var stroke1 = lineSeries1.Stroke;
        var stroke2 = lineSeries2.Stroke;
        Assert.That(stroke1, Is.Not.Null, "First line should have a stroke");
        Assert.That(stroke2, Is.Not.Null, "Second line should have a stroke");
        Assert.That(stroke1, Is.Not.SameAs(stroke2), "Lines should have different strokes");
    }

    #endregion
}