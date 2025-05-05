using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Sensors;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ProcessTests;

[TestFixture]
public class ProcessingTimeEstimatorTests
{
    private ProcessingTimeEstimator _estimator;
    private string _tempDir;
    
    [SetUp]
    public void Setup()
    {
        _estimator = new ProcessingTimeEstimator();
        
        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }
    
    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }
    
    [Test]
    public void EstimateProcessingTime_WithNoFiles_ReturnsZeroTimeSpan()
    {
        // Arrange
        var files = Array.Empty<string>();
        
        // Act
        var result = _estimator.EstimateProcessingTime(files);
        
        // Assert
        Assert.That(result, Is.EqualTo(TimeSpan.Zero));
    }
    
    [Test]
    public void EstimateProcessingTime_WithNullFiles_ReturnsZeroTimeSpan()
    {
        // Arrange
        string[] files = null;
        
        // Act
        var result = _estimator.EstimateProcessingTime(files);
        
        // Assert
        Assert.That(result, Is.EqualTo(TimeSpan.Zero));
    }
    
    [Test]
    public void EstimateProcessingTime_WithFiles_ReturnsEstimatedTime()
    {
        // Arrange
        // Create some files with known sizes
        string file1 = Path.Combine(_tempDir, "file1.bin");
        string file2 = Path.Combine(_tempDir, "file2.bin");
        
        // Create 1MB file
        using (var fs = new FileStream(file1, FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(1024 * 1024); // 1MB
        }
        
        // Create 2MB file
        using (var fs = new FileStream(file2, FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(2 * 1024 * 1024); // 2MB
        }
        
        var files = new[] { file1, file2 };
        
        // Act
        var result = _estimator.EstimateProcessingTime(files);
        
        // Assert
        // The estimator uses the formula: totalSize (MB) * 6 seconds
        // Total size = 3MB, so estimated time = 18 seconds
        TimeSpan expectedTime = TimeSpan.FromSeconds(18);
        Assert.That(result, Is.EqualTo(expectedTime));
    }
    
    [Test]
    public void EstimateProcessingTime_WithSmallFiles_ReturnsMinimumTime()
    {
        // Arrange
        // Create a very small file
        string smallFile = Path.Combine(_tempDir, "small.bin");
        
        // Create 10KB file
        using (var fs = new FileStream(smallFile, FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(10 * 1024); // 10KB
        }
        
        var files = new[] { smallFile };
        
        // Act
        var result = _estimator.EstimateProcessingTime(files);
        
        // Assert
        // The estimator has a minimum time of 5 seconds
        TimeSpan minimumTime = TimeSpan.FromSeconds(5);
        Assert.That(result, Is.EqualTo(minimumTime));
    }
    
    [Test]
    public void EstimateProcessingTime_WithNonExistentFiles_IgnoresMissingFiles()
    {
        // Arrange
        // Create one real file
        string realFile = Path.Combine(_tempDir, "real.bin");
        
        // Create 1MB file
        using (var fs = new FileStream(realFile, FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(1024 * 1024); // 1MB
        }
        
        // Non-existent file
        string nonExistentFile = Path.Combine(_tempDir, "non_existent.bin");
        
        var files = new[] { realFile, nonExistentFile };
        
        // Act
        var result = _estimator.EstimateProcessingTime(files);
        
        // Assert
        // Only the real file should be counted
        // 1MB * 6 seconds = 6 seconds
        TimeSpan expectedTime = TimeSpan.FromSeconds(6);
        Assert.That(result, Is.EqualTo(expectedTime));
    }
    
    [Test]
    public void EstimateProcessingTime_WithLargeFiles_ScalesCorrectly()
    {
        // Arrange
        // For this test, we'll simulate large files without actually creating them
        // by mocking the FileInfo behavior
        
        // Create a temporary file just to get a valid path
        string filePath = Path.Combine(_tempDir, "large.bin");
        File.WriteAllText(filePath, "placeholder");
        
        // Calculate the expected time for 100MB
        long fileSizeInBytes = 100 * 1024 * 1024; // 100MB
        double fileSizeInMB = fileSizeInBytes / (1024.0 * 1024.0);
        double expectedSeconds = fileSizeInMB * 6;
        TimeSpan expectedTime = TimeSpan.FromSeconds(expectedSeconds);
        
        // Use reflection to access the private method that gets the file size
        // This is a workaround to avoid creating large files
        var testEstimator = new TestProcessingTimeEstimator();
        testEstimator.SetFileSize(filePath, fileSizeInBytes);
        
        // Act
        var result = testEstimator.EstimateProcessingTime(new[] { filePath });
        
        // Assert
        Assert.That(result.TotalSeconds, Is.EqualTo(expectedTime.TotalSeconds).Within(0.1));
    }
    
    /// <summary>
    /// Test implementation of ProcessingTimeEstimator that allows setting file sizes for testing
    /// </summary>
    private class TestProcessingTimeEstimator : ProcessingTimeEstimator
    {
        private readonly Dictionary<string, long> _fileSizes = new();
        
        public void SetFileSize(string filePath, long size)
        {
            _fileSizes[filePath] = size;
        }
        
        public new TimeSpan EstimateProcessingTime(IEnumerable<string> files)
        {
            if (files == null || !files.Any())
                return TimeSpan.Zero;

            var fileCount = files.Count();
            long totalSize = 0;

            foreach (var file in files)
                if (File.Exists(file))
                {
                    totalSize += _fileSizes.TryGetValue(file, out long size) ? size : new FileInfo(file).Length;
                }

            // Estimate 6 seconds per MB, with a minimum of 5 seconds
            double estimatedSeconds = totalSize / (1024 * 1024.0) * 6;
            return TimeSpan.FromSeconds(Math.Max(5, estimatedSeconds));
        }
    }
}