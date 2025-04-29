using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using Newtonsoft.Json;

namespace ActiveSense.Desktop.Sensors;

public class AnalysisSerializer(DateToWeekdayConverter converter)
{
    public string ExportToBase64(IAnalysis analysis)
    {
        if (analysis == null)
            throw new ArgumentNullException(nameof(analysis));

        try
        {
            var serializable = new SerializableAnalysis
            {
                FileName = analysis.FileName,
                FilePath = analysis.FilePath,
                ActivityRecords = analysis is IActivityAnalysis activityAnalysis
                    ? activityAnalysis.ActivityRecords
                    : null,
                SleepRecords = analysis is ISleepAnalysis sleepAnalysis ? sleepAnalysis.SleepRecords : null
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

    public IAnalysis ImportFromBase64(string base64)
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
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public IEnumerable<ActivityRecord> ActivityRecords { get; set; }
        public IEnumerable<SleepRecord> SleepRecords { get; set; }
    }
}