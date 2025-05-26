using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Process;

public class ProcessingTimeEstimator : IProcessingTimeEstimator
{
    private const double BenchmarkFileSizeMb = 767.7;
    private const double BenchmarkTimeSeconds = 388;
    private const double BaselineMbPerSecond = BenchmarkFileSizeMb / BenchmarkTimeSeconds;

    private const int CalibrationIterations = 1_000_000;
    private const int CalibrationDataSizeBytes = 1024;

    // The reference calibration time is the time taken for the calibration task on the machine that has the above benchmark speed.
    private const double ReferenceCalibrationTimeSeconds = 1.239;

    private static readonly Lazy<double> MachineSpeedFactor =
        new(CalculateMachineSpeedFactor, LazyThreadSafetyMode.ExecutionAndPublication);

    public TimeSpan EstimateProcessingTime(double totalFileSizesMB, IList<ScriptArgument> arguments)
    {
        if (totalFileSizesMB <= 10) return TimeSpan.Zero;

        var estimatedSecondsOnBenchmarkMachine = totalFileSizesMB / BaselineMbPerSecond;

        var actualEstimatedSeconds = estimatedSecondsOnBenchmarkMachine * MachineSpeedFactor.Value;

        if (actualEstimatedSeconds < 0) actualEstimatedSeconds = 0;

        // Apply reduction based on disabled analysis types
        var reductionFactor = GetReductionFactor(arguments);
        actualEstimatedSeconds *= reductionFactor;

        if (totalFileSizesMB < 20) return TimeSpan.FromSeconds(actualEstimatedSeconds + 15);

        return TimeSpan.FromSeconds(actualEstimatedSeconds);
    }

    private static double GetReductionFactor(IList<ScriptArgument> arguments)
    {
        if (arguments == null || arguments.Count == 0) return 1.0;

        var activityEnabled = true;
        var sleepEnabled = true;
        var legacyEnabled = false;

        foreach (var arg in arguments)
            if (arg is BoolArgument boolArg)
            {
                if (boolArg.Flag == "activity")
                    activityEnabled = boolArg.Value;
                else if (boolArg.Flag == "sleep")
                    sleepEnabled = boolArg.Value;
                else if (boolArg.Flag == "legacy") legacyEnabled = boolArg.Value;
            }

        double baseFactor;
        if (activityEnabled && sleepEnabled)
            baseFactor = 1.0;
        else if (activityEnabled || sleepEnabled)
            baseFactor = 0.5;
        else
            baseFactor = 0.1;

        if (legacyEnabled) baseFactor *= 2.0; // Legacy mode takes double the time

        return baseFactor;
    }

    private static double CalculateMachineSpeedFactor()
    {
        var values = new List<double>();

        for (var i = 0; i < 4; i++) values.Add(RunCalibrationTaskInternal());

        var average = values.Average();
        if (average <= 0) return 1.0;

        var factor = average / ReferenceCalibrationTimeSeconds;

        factor = Math.Max(0.2, Math.Min(5.0, factor));

        return factor;
    }

    public static double RunCalibrationTaskInternal()
    {
        var data = new byte[CalibrationDataSizeBytes];
        new Random(42).NextBytes(data);

        var sw = Stopwatch.StartNew();
        using (var sha256 = SHA256.Create())
        {
            for (var i = 0; i < CalibrationIterations; i++)
            {
                data[0] = (byte)(i % 256);
                sha256.ComputeHash(data);
            }
        }

        sw.Stop();
        return sw.Elapsed.TotalSeconds;
    }

    // Helper method to test calibration
    public static void TestCalibration()
    {
        Console.WriteLine(
            $"Running calibration task with {CalibrationIterations} iterations on {CalibrationDataSizeBytes} byte blocks...");
        var values = new List<double>();
        for (var i = 0; i < 5; i++)
        {
            Console.WriteLine($"Iteration {i + 1}...");
            values.Add(RunCalibrationTaskInternal());
        }

        Console.WriteLine($"Average time: {values.Average():F3}");
    }
}