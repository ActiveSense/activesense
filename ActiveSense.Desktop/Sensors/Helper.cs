using System;
using System.Text;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Models;
using Newtonsoft.Json;

namespace ActiveSense.Desktop.Sensors;

public class AnalysisSerializer(DateToWeekdayConverter converter)
{
    public string ExportToBase64(Analysis analysis)
    {
        if (analysis == null)
            throw new ArgumentNullException(nameof(analysis));

        try
        {
            var serializable = new SerializableAnalysis
            {
                FileName = analysis.FileName,
                FilePath = analysis.FilePath,
                ActivityRecords = analysis.ActivityRecords,
                SleepRecords = analysis.SleepRecords
            };

            string json = JsonConvert.SerializeObject(serializable, Formatting.None);
            
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to serialize Analysis object: {ex.Message}", ex);
        }
    }

    public Analysis ImportFromBase64(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            throw new ArgumentNullException(nameof(base64));

        try
        {
            byte[] bytes = Convert.FromBase64String(base64);
            string json = Encoding.UTF8.GetString(bytes);
            
            var serializable = JsonConvert.DeserializeObject<SerializableAnalysis>(json);
            
            if (serializable == null)
                throw new Exception("Deserialization resulted in a null object");
            
            var analysis = new Analysis(converter)
            {
                FileName = serializable.FileName,
                FilePath = serializable.FilePath
            };
            
            analysis.SetActivityRecords(serializable.ActivityRecords);
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
        public System.Collections.Generic.IEnumerable<ActivityRecord> ActivityRecords { get; set; }
        public System.Collections.Generic.IEnumerable<SleepRecord> SleepRecords { get; set; }
    }
}