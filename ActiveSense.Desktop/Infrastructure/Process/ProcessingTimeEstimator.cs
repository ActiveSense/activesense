using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Process.Interfaces;

namespace ActiveSense.Desktop.Sensors;

public class ProcessingTimeEstimator : IProcessingTimeEstimator
{
    public TimeSpan EstimateProcessingTime(IEnumerable<string> files)
    {
        if (files == null || !files.Any())
            return TimeSpan.Zero;

        var fileCount = files.Count();
        long totalSize = 0;

        foreach (var file in files)
            if (File.Exists(file))
            {
                var fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }

        // Estimate 6 seconds per MB, with a minimum of 5 seconds
        double estimatedSeconds = totalSize / (1024 * 1024) * 6;
        return TimeSpan.FromSeconds(Math.Max(5, estimatedSeconds));
    }
}