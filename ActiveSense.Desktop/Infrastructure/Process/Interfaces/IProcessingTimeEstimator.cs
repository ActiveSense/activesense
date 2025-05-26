using System;
using System.Collections.Generic;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;

namespace ActiveSense.Desktop.Infrastructure.Process.Interfaces;

public interface IProcessingTimeEstimator
{
    TimeSpan EstimateProcessingTime(double totalSizeMB, IList<ScriptArgument> arguments);
}