using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Process;

    public class ProcessingTimeEstimator : IProcessingTimeEstimator
{
    // --- Baseline Benchmark Data ---
    private const double BENCHMARK_FILE_SIZE_MB = 750.0;
    private const double BENCHMARK_TIME_SECONDS = 6.0 * 60.0; // 360 seconds
    private const double BASELINE_MB_PER_SECOND = BENCHMARK_FILE_SIZE_MB / BENCHMARK_TIME_SECONDS; // MB/s on the "good" benchmark machine

    // --- Calibration Configuration ---
    // Number of iterations for the calibration task.
    // Adjust this so the task takes a few seconds (e.g., 1-5s) on an average machine.
    private const int CALIBRATION_ITERATIONS = 1_500_000; // Example: 500k iterations
    private const int CALIBRATION_DATA_SIZE_BYTES = 1024; // 1KB data block for hashing

    // **IMPORTANT**: This is the time (in seconds) the `RunCalibrationTask` (with the above settings)
    // is expected to take on the "good" benchmark machine (the one that processes 750MB in 6 minutes).
    // You MUST determine this value by:
    // 1. Running `RunCalibrationTaskInternal()` on a machine that matches your "good machine" profile.
    // 2. Or, run it on your dev machine, note the time, and then estimate.
    //    E.g., if it takes 1.5s on your fast dev machine, and you think the "good machine" is 2x slower,
    //    then REFERENCE_CALIBRATION_TIME_SECONDS would be 3.0.
    private const double REFERENCE_CALIBRATION_TIME_SECONDS = 2.127; // <<< !!! ADJUST THIS VALUE !!!

    private static readonly Lazy<double> _machineSpeedFactor = new Lazy<double>(CalculateMachineSpeedFactor, LazyThreadSafetyMode.ExecutionAndPublication);

    public TimeSpan EstimateProcessingTime(IEnumerable<string> files)
    {
        if (files == null || !files.Any())
        {
            return TimeSpan.Zero;
        }

        long totalSizeBytes = 0;
        foreach (string filePath in files)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    totalSizeBytes += new FileInfo(filePath).Length;
                }
                catch (Exception ex) // Catch potential IOExceptions, SecurityExceptions etc.
                {
                    // Log this error or handle as appropriate
                    Console.WriteLine($"Warning: Could not get size for file {filePath}. Error: {ex.Message}");
                }
            }
        }

        if (totalSizeBytes == 0)
        {
            return TimeSpan.Zero;
        }

        double totalSizeMB = totalSizeBytes / (1024.0 * 1024.0);

        // Estimated time if this machine were the "good" benchmark machine
        double estimatedSecondsOnBenchmarkMachine = totalSizeMB / BASELINE_MB_PER_SECOND;

        // Adjust for this machine's actual speed factor
        double actualEstimatedSeconds = estimatedSecondsOnBenchmarkMachine * _machineSpeedFactor.Value;

        // Sanity check: don't return negative or excessively small times if something went weird.
        if (actualEstimatedSeconds < 0) actualEstimatedSeconds = 0;

        return TimeSpan.FromSeconds(actualEstimatedSeconds);
    }

    private static double CalculateMachineSpeedFactor()
    {
        Console.WriteLine("Performing one-time system performance calibration for time estimation...");
        Stopwatch sw = Stopwatch.StartNew();
        
        double currentMachineCalibrationTime = RunCalibrationTaskInternal();
        
        sw.Stop();
        Console.WriteLine($"Calibration task completed in {currentMachineCalibrationTime:F2} seconds (Total time including overhead: {sw.Elapsed.TotalSeconds:F2}s).");

        if (REFERENCE_CALIBRATION_TIME_SECONDS <= 0)
        {
            Console.WriteLine("Warning: REFERENCE_CALIBRATION_TIME_SECONDS is invalid. Assuming default speed factor of 1.0.");
            return 1.0; // Avoid division by zero / invalid reference
        }
        if (currentMachineCalibrationTime <=0)
        {
            Console.WriteLine("Warning: Calibration task returned non-positive time. Assuming default speed factor of 1.0.");
            return 1.0;
        }


        double factor = currentMachineCalibrationTime / REFERENCE_CALIBRATION_TIME_SECONDS;

        // Optional: Clamp the factor to avoid extreme values if calibration is noisy
        // or if the machine is exceptionally fast/slow.
        // For example, clamp between 0.2 (5x faster) and 5.0 (5x slower).
        factor = Math.Max(0.2, Math.Min(5.0, factor));
        
        Console.WriteLine($"Calculated machine speed factor: {factor:F2} (Reference time: {REFERENCE_CALIBRATION_TIME_SECONDS:F2}s, This machine time: {currentMachineCalibrationTime:F2}s)");


        return factor;
    }

    // This is the actual CPU-bound task.
    // It's public static so you can run it independently on your "good" machine
    // to determine the REFERENCE_CALIBRATION_TIME_SECONDS.
    public static double RunCalibrationTaskInternal()
    {
        byte[] data = new byte[CALIBRATION_DATA_SIZE_BYTES];
        new Random(42).NextBytes(data); // Seeded for deterministic data, though not strictly necessary for hash timing

        Stopwatch sw = Stopwatch.StartNew();
        using (SHA256 sha256 = SHA256.Create())
        {
            for (int i = 0; i < CALIBRATION_ITERATIONS; i++)
            {
                // Slightly vary input data to prevent extreme JIT optimizations or caching effects
                // if the hash algorithm itself had some internal state not reset by ComputeHash.
                // For SHA256, ComputeHash is stateless, but this is a good practice.
                data[0] = (byte)(i % 256); 
                sha256.ComputeHash(data);
            }
        }
        sw.Stop();
        return sw.Elapsed.TotalSeconds;
    }

    // Helper method if you want to test calibration externally
    public static void TestCalibration()
    {
        Console.WriteLine($"Running calibration task with {CALIBRATION_ITERATIONS} iterations on {CALIBRATION_DATA_SIZE_BYTES} byte blocks...");
        double timeTaken = RunCalibrationTaskInternal();
        Console.WriteLine($"Calibration task took: {timeTaken:F3} seconds on this machine.");
        Console.WriteLine($"If this is your 'good' benchmark machine, set REFERENCE_CALIBRATION_TIME_SECONDS to ~{timeTaken:F3}.");
    }
}