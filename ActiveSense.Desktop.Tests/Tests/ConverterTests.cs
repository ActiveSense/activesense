using System;
using ActiveSense.Desktop.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ActiveSense.Desktop.Tests.Tests;

[TestClass]
public class ConverterTests
{
    private DateToWeekdayConverter converter = new DateToWeekdayConverter();
    string date = "2023-10-01";
    
    [TestMethod]
    public void TestDateToWeekdayConverter()
    {
        var result = converter.ConvertStringToDate(date);
        Assert.AreEqual(new DateTime(2023, 10, 1), result);
    }
}