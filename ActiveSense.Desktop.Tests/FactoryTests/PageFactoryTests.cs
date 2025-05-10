using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.ViewModels;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.FactoryTests;

[TestFixture]
public class PageFactoryTests
{
    [SetUp]
    public void Setup()
    {
        // Create mocks for different page view models
        _mockAnalysisPageViewModel = new Mock<PageViewModel>();
        _mockSleepPageViewModel = new Mock<PageViewModel>();
        _mockActivityPageViewModel = new Mock<PageViewModel>();
        _mockGeneralPageViewModel = new Mock<PageViewModel>();

        // Configure mock factory to return different view models based on page name
        _mockFactory = new Mock<Func<ApplicationPageNames, PageViewModel>>();
        _mockFactory.Setup(f => f(ApplicationPageNames.Analyse)).Returns(_mockAnalysisPageViewModel.Object);
        _mockFactory.Setup(f => f(ApplicationPageNames.Sleep)).Returns(_mockSleepPageViewModel.Object);
        _mockFactory.Setup(f => f(ApplicationPageNames.Activity)).Returns(_mockActivityPageViewModel.Object);
        _mockFactory.Setup(f => f(ApplicationPageNames.General)).Returns(_mockGeneralPageViewModel.Object);

        // Throw for unknown page names to test error handling
        _mockFactory.Setup(f => f(ApplicationPageNames.Unknown)).Throws<InvalidOperationException>();

        // Create the factory with the mock function
        _pageFactory = new PageFactory(_mockFactory.Object);
    }

    private PageFactory _pageFactory;
    private Mock<Func<ApplicationPageNames, PageViewModel>> _mockFactory;
    private Mock<PageViewModel> _mockAnalysisPageViewModel;
    private Mock<PageViewModel> _mockSleepPageViewModel;
    private Mock<PageViewModel> _mockActivityPageViewModel;
    private Mock<PageViewModel> _mockGeneralPageViewModel;

    [Test]
    public void GetPageViewModel_ForAnalysePage_ReturnsAnalyseViewModel()
    {
        // Act
        var result = _pageFactory.GetPageViewModel(ApplicationPageNames.Analyse);

        // Assert
        Assert.That(result, Is.SameAs(_mockAnalysisPageViewModel.Object));
        _mockFactory.Verify(f => f(ApplicationPageNames.Analyse), Times.Once);
    }

    [Test]
    public void GetPageViewModel_ForSleepPage_ReturnsSleepViewModel()
    {
        // Act
        var result = _pageFactory.GetPageViewModel(ApplicationPageNames.Sleep);

        // Assert
        Assert.That(result, Is.SameAs(_mockSleepPageViewModel.Object));
        _mockFactory.Verify(f => f(ApplicationPageNames.Sleep), Times.Once);
    }

    [Test]
    public void GetPageViewModel_ForActivityPage_ReturnsActivityViewModel()
    {
        // Act
        var result = _pageFactory.GetPageViewModel(ApplicationPageNames.Activity);

        // Assert
        Assert.That(result, Is.SameAs(_mockActivityPageViewModel.Object));
        _mockFactory.Verify(f => f(ApplicationPageNames.Activity), Times.Once);
    }

    [Test]
    public void GetPageViewModel_ForGeneralPage_ReturnsGeneralViewModel()
    {
        // Act
        var result = _pageFactory.GetPageViewModel(ApplicationPageNames.General);

        // Assert
        Assert.That(result, Is.SameAs(_mockGeneralPageViewModel.Object));
        _mockFactory.Verify(f => f(ApplicationPageNames.General), Times.Once);
    }

    [Test]
    public void GetPageViewModel_ForUnknownPage_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _pageFactory.GetPageViewModel(ApplicationPageNames.Unknown));
        _mockFactory.Verify(f => f(ApplicationPageNames.Unknown), Times.Once);
    }

    [Test]
    public void GetPageViewModel_ForUploadPage_InvokesCorrectFactory()
    {
        // Arrange
        _mockFactory.Setup(f => f(ApplicationPageNames.Upload)).Returns(_mockAnalysisPageViewModel.Object);

        // Act
        var result = _pageFactory.GetPageViewModel(ApplicationPageNames.Upload);

        // Assert
        Assert.That(result, Is.SameAs(_mockAnalysisPageViewModel.Object));
        _mockFactory.Verify(f => f(ApplicationPageNames.Upload), Times.Once);
    }

    [Test]
    public void GetPageViewModel_AllDefinedPageNames_CanBeResolved()
    {
        // Test that we can resolve all page names in the enum
        // (except Unknown, which we already tested separately)
        foreach (ApplicationPageNames pageName in Enum.GetValues(typeof(ApplicationPageNames)))
            if (pageName != ApplicationPageNames.Unknown)
            {
                _mockFactory.Setup(f => f(pageName)).Returns(_mockAnalysisPageViewModel.Object);

                // Act - should not throw
                var result = _pageFactory.GetPageViewModel(pageName);

                // Assert
                Assert.That(result, Is.SameAs(_mockAnalysisPageViewModel.Object));
                _mockFactory.Verify(f => f(pageName), Times.Once);

                // Reset for next iteration
                _mockFactory.Invocations.Clear();
            }
    }
}