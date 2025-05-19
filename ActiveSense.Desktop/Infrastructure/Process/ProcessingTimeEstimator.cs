using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
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
    private const double ReferenceCalibrationTimeSeconds = 1.178;

    private static readonly Lazy<double> MachineSpeedFactor = new Lazy<double>(CalculateMachineSpeedFactor, LazyThreadSafetyMode.ExecutionAndPublication);

    public TimeSpan EstimateProcessingTime(double totalFileSizesMB)
    {

        double estimatedSecondsOnBenchmarkMachine = totalFileSizesMB / BaselineMbPerSecond;

        double actualEstimatedSeconds = estimatedSecondsOnBenchmarkMachine * MachineSpeedFactor.Value;

        if (actualEstimatedSeconds < 0) actualEstimatedSeconds = 0;

        return TimeSpan.FromSeconds(actualEstimatedSeconds);
    }

    private static double CalculateMachineSpeedFactor()
    {
        Stopwatch sw = Stopwatch.StartNew();
        double currentMachineCalibrationTime = RunCalibrationTaskInternal();
        sw.Stop();

        if (currentMachineCalibrationTime <=0)
        {
            return 1.0;
        }

        double factor = currentMachineCalibrationTime / ReferenceCalibrationTimeSeconds;

        factor = Math.Max(0.2, Math.Min(5.0, factor));

        return factor;
    }

    public static double RunCalibrationTaskInternal()
    {
        byte[] data = new byte[CalibrationDataSizeBytes];
        new Random(42).NextBytes(data);

        Stopwatch sw = Stopwatch.StartNew();
        using (SHA256 sha256 = SHA256.Create())
        {
            for (int i = 0; i < CalibrationIterations; i++)
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
        Console.WriteLine($"Running calibration task with {CalibrationIterations} iterations on {CalibrationDataSizeBytes} byte blocks...");
        double timeTaken = RunCalibrationTaskInternal();
        Console.WriteLine($"Calibration task took: {timeTaken:F3} seconds on this machine.");
        Console.WriteLine($"If this is your 'good' benchmark machine, set REFERENCE_CALIBRATION_TIME_SECONDS to ~{timeTaken:F3}.");
    }
}