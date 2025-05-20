using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Core.Services;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.CoreTests.ServicesTests;

[TestFixture]
public class SharedDataServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockDateConverter = new Mock<DateToWeekdayConverter>();
        _sharedDataService = new SharedDataService();

        // Create test analyses
        _testAnalyses = new List<IAnalysis>
        {
            CreateMockAnalysis("Analysis1", "/path/to/analysis1"),
            CreateMockAnalysis("Analysis2", "/path/to/analysis2"),
            CreateMockAnalysis("Analysis3", "/path/to/analysis3")
        };
    }

    private Mock<DateToWeekdayConverter> _mockDateConverter;
    private SharedDataService _sharedDataService;
    private List<IAnalysis> _testAnalyses;

    private IAnalysis CreateMockAnalysis(string fileName, string filePath)
    {
        var analysis = new GeneActiveAnalysis(_mockDateConverter.Object)
        {
            FileName = fileName,
            FilePath = filePath
        };
        return analysis;
    }

    [Test]
    public void UpdateSelectedAnalyses_WithNewAnalyses_UpdatesCollection()
    {
        // Arrange
        var eventRaised = false;
        _sharedDataService.SelectedAnalysesChanged += (sender, e) => eventRaised = true;

        var selectedAnalyses = new ObservableCollection<IAnalysis>
        {
            _testAnalyses[0],
            _testAnalyses[1]
        };

        // Act
        _sharedDataService.UpdateSelectedAnalyses(selectedAnalyses);

        // Assert
        Assert.That(_sharedDataService.SelectedAnalyses.Count, Is.EqualTo(2));
        Assert.That(_sharedDataService.SelectedAnalyses, Contains.Item(_testAnalyses[0]));
        Assert.That(_sharedDataService.SelectedAnalyses, Contains.Item(_testAnalyses[1]));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void UpdateAllAnalyses_WithNewAnalyses_AddsToCollection()
    {
        // Arrange
        var eventRaised = false;
        _sharedDataService.AllAnalysesChanged += (sender, e) => eventRaised = true;

        // Act
        _sharedDataService.UpdateAllAnalyses(_testAnalyses);

        // Assert
        Assert.That(_sharedDataService.AllAnalyses.Count, Is.EqualTo(3));
        Assert.That(_sharedDataService.AllAnalyses, Contains.Item(_testAnalyses[0]));
        Assert.That(_sharedDataService.AllAnalyses, Contains.Item(_testAnalyses[1]));
        Assert.That(_sharedDataService.AllAnalyses, Contains.Item(_testAnalyses[2]));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void UpdateAllAnalyses_WithExistingItems_ReplacesItems()
    {
        // Arrange
        _sharedDataService.UpdateAllAnalyses(_testAnalyses);

        // Create a modified version of the first analysis
        var updatedAnalysis = CreateMockAnalysis("Analysis1", "/updated/path");

        var eventRaised = false;
        _sharedDataService.AllAnalysesChanged += (sender, e) => eventRaised = true;

        // Act
        _sharedDataService.UpdateAllAnalyses(new[] { updatedAnalysis });

        // Assert
        Assert.That(_sharedDataService.AllAnalyses.Count, Is.EqualTo(3), "Count should stay the same");

        var updatedItem = _sharedDataService.AllAnalyses.FirstOrDefault(a => a.FileName == "Analysis1");
        Assert.That(updatedItem, Is.Not.Null);
        Assert.That(updatedItem.FilePath, Is.EqualTo("/updated/path"));

        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void IsProcessingInBackground_ChangingValue_TriggersEvent()
    {
        // Arrange
        var eventRaised = false;
        _sharedDataService.BackgroundProcessingChanged += (sender, e) => eventRaised = true;

        // Act
        _sharedDataService.IsProcessingInBackground = true;

        // Assert
        Assert.That(_sharedDataService.IsProcessingInBackground, Is.True);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void IsProcessingInBackground_SettingSameValue_DoesNotTriggerEvent()
    {
        // Arrange
        _sharedDataService.IsProcessingInBackground = true;

        var eventRaised = false;
        _sharedDataService.BackgroundProcessingChanged += (sender, e) => eventRaised = true;

        // Act
        _sharedDataService.IsProcessingInBackground = true;

        // Assert
        Assert.That(eventRaised, Is.False);
    }
}