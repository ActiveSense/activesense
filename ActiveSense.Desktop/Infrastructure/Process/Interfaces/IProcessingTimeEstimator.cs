using System;
using System.Collections.Generic;

namespace ActiveSense.Desktop.Infrastructure.Process.Interfaces;

public interface IProcessingTimeEstimator
{
    TimeSpan EstimateProcessingTime(double totalSizeMB);
}