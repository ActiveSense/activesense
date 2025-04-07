using System;
using NUnit.Framework;
using ActiveSense.Desktop;

namespace ActiveSense.Desktop.Tests.Tests;

[TestFixture]
public class PathTests
{

    
    [Test]
    public void TestSolutionBasePath()
    {
        Console.WriteLine(AppConfig.SolutionBasePath);
    }
}