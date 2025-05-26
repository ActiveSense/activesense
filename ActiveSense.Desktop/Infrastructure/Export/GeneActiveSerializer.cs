using System;
using System.Collections.Generic;
using System.Text;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using Newtonsoft.Json;

namespace ActiveSense.Desktop.Infrastructure.Export;

public class AnalysisSerializer(DateToWeekdayConverter converter) : IAnalysisSerializer
{
    public virtual string ExportToBase64(IAnalysis analysis)
    {
        if (analysis is not (IActivityAnalysis activityAnalysis and ISleepAnalysis sleepAnalysis))
            throw new ArgumentException("Analysis must be an IActivityAnalysis or ISleepAnalysis.");

        try
        {
            var serializable = new SerializableAnalysis
            {
                FileName = analysis.FileName,
                FilePath = analysis.FilePath,
                ActivityRecords = activityAnalysis.ActivityRecords,
                SleepRecords = sleepAnalysis.SleepRecords
            };

            var json = JsonConvert.SerializeObject(serializable, Formatting.None);

            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to serialize Analysis object: {ex.Message}", ex);
        }
    }

    public virtual IAnalysis ImportFromBase64(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            throw new ArgumentNullException(nameof(base64));

        try
        {
            var bytes = Convert.FromBase64String(base64);
            var json = Encoding.UTF8.GetString(bytes);

            var serializable = JsonConvert.DeserializeObject<SerializableAnalysis>(json);

            if (serializable == null)
                throw new Exception("Deserialization resulted in a null object");

            var analysis = new GeneActiveAnalysis(converter)
            {
                FileName = serializable.FileName,
                FilePath = serializable.FilePath
            };

            if (serializable.ActivityRecords != null)
                analysis.SetActivityRecords(serializable.ActivityRecords);

            if (serializable.SleepRecords != null)
                analysis.SetSleepRecords(serializable.SleepRecords);

            return analysis;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to deserialize Analysis object: {ex.Message}", ex);
        }
    }

    private class SerializableAnalysis
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public IEnumerable<ActivityRecord> ActivityRecords { get; set; } = new List<ActivityRecord>();
        public IEnumerable<SleepRecord> SleepRecords { get; set; } = new List<SleepRecord>();
    }
}