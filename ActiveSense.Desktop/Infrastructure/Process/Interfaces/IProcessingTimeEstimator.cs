using System;
using System.Collections.Generic;

namespace ActiveSense.Desktop.Process.Interfaces;

public interface IProcessingTimeEstimator
{
    TimeSpan EstimateProcessingTime(IEnumerable<string> files);
}