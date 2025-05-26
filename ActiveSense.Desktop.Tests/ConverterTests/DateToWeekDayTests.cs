using System;
using System.Globalization;
using ActiveSense.Desktop.Converters;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ConverterTests;

[TestFixture]
public class DateToWeekdayConverterTests
{
    [SetUp]
    public void Setup()
    {
        _converter = new DateToWeekdayConverter();
    }

    private DateToWeekdayConverter _converter;

    [Test]
    public void ConvertDateToWeekday_WithValidDateString_ReturnsCorrectGermanWeekday()
    {
        // Arrange
        var dateString = "2024-05-20"; // This is a Tuesday (Dienstag)

        // Act
        var result = _converter.ConvertDateToWeekday(dateString);

        // Assert
        Assert.That(result, Is.EqualTo("Montag"), "Should return 'Montag' for 2024-05-20");
    }

    [Test]
    public void ConvertDateToWeekday_WithWeekendDate_ReturnsCorrectGermanWeekday()
    {
        // Arrange
        var saturdayString = "2024-05-18"; // This is a Saturday
        var sundayString = "2024-05-19"; // This is a Sunday

        // Act
        var saturdayResult = _converter.ConvertDateToWeekday(saturdayString);
        var sundayResult = _converter.ConvertDateToWeekday(sundayString);

        // Assert
        Assert.That(saturdayResult, Is.EqualTo("Samstag"), "Should return 'Samstag' for 2024-05-18");
        Assert.That(sundayResult, Is.EqualTo("Sonntag"), "Should return 'Sonntag' for 2024-05-19");
    }

    [Test]
    public void ConvertDateToWeekday_WithAllWeekdays_ReturnsAllGermanWeekdays()
    {
        // Test all days of the week starting from Monday 2024-05-20
        string[] dateStrings =
        {
            "2024-05-20", // Monday
            "2024-05-21", // Tuesday
            "2024-05-22", // Wednesday
            "2024-05-23", // Thursday
            "2024-05-24", // Friday
            "2024-05-25", // Saturday
            "2024-05-26" // Sunday
        };

        string[] expectedWeekdays =
        {
            "Montag",
            "Dienstag",
            "Mittwoch",
            "Donnerstag",
            "Freitag",
            "Samstag",
            "Sonntag"
        };

        // Test each date string
        for (var i = 0; i < dateStrings.Length; i++)
        {
            var result = _converter.ConvertDateToWeekday(dateStrings[i]);
            Assert.That(result, Is.EqualTo(expectedWeekdays[i]),
                $"Should return '{expectedWeekdays[i]}' for {dateStrings[i]}");
        }
    }

    [Test]
    public void ConvertDateToWeekday_WithInvalidDateFormat_ThrowsException()
    {
        // Arrange
        var invalidDateString = "not-a-date";

        // Act & Assert
        Assert.Throws<Exception>(() => _converter.ConvertDateToWeekday(invalidDateString),
            "Should throw an exception for invalid date format");
    }

    [Test]
    public void ConvertDateToWeekday_WithNullDate_ThrowsException()
    {
        // Arrange
        string nullDateString = null;

        // Act & Assert
        Assert.Throws<Exception>(() => _converter.ConvertDateToWeekday(nullDateString),
            "Should throw an exception for null date");
    }

    [Test]
    public void ConvertDateToWeekday_WithEmptyDate_ThrowsException()
    {
        // Arrange
        var emptyDateString = "";

        // Act & Assert
        Assert.Throws<Exception>(() => _converter.ConvertDateToWeekday(emptyDateString),
            "Should throw an exception for empty date");
    }

    [Test]
    public void ConvertDateToWeekday_DateObjectConsistency_ReturnsConsistentWeekdays()
    {
        // This test verifies that the converter returns weekdays consistent with DateTime.DayOfWeek
        // by creating a DateTime object and comparing its German weekday name with the converter output

        // Arrange - create a date and convert it to string format
        var testDate = new DateTime(2024, 5, 20); // A Monday
        var dateString = testDate.ToString("yyyy-MM-dd");

        // Create expected German weekday from DateTime's DayOfWeek
        var germanCulture = new CultureInfo("de-DE");
        var expectedWeekday = germanCulture.DateTimeFormat.GetDayName(testDate.DayOfWeek);

        // Act
        var result = _converter.ConvertDateToWeekday(dateString);

        // Assert
        Assert.That(result, Is.EqualTo(expectedWeekday),
            "Converter should return same weekday as DateTime.DayOfWeek in German culture");
    }
}