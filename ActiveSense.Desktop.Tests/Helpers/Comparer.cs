using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace ActiveSense.Desktop.Tests.Helpers;

public class Comparer
{
    public bool CompareFiles(string file1, string file2)
    {
        if (AreFilesIdentical(file1, file2))
            return true;

        var records1 = ReadCsvRecords(file1);
        var records2 = ReadCsvRecords(file2);

        if (records1.Count != records2.Count)
            return false;

        for (var i = 0; i < records1.Count; i++)
            if (!CompareRecords(records1[i], records2[i]))
                return false;

        return true;
    }

    private bool AreFilesIdentical(string file1, string file2)
    {
        var content1 = File.ReadAllText(file1);
        var content2 = File.ReadAllText(file2);
        return content1 == content2;
    }

    private List<Dictionary<string, string>> ReadCsvRecords(string filePath)
    {
        var records = new List<Dictionary<string, string>>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null
        };

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            while (csv.Read())
            {
                var record = new Dictionary<string, string>();

                foreach (var header in headers)
                {
                    var value = csv.GetField(header) ?? string.Empty;
                    record[header] = value;
                }

                records.Add(record);
            }
        }

        return records;
    }

    private bool CompareRecords(Dictionary<string, string> record1, Dictionary<string, string> record2)
    {
        if (record1.Count != record2.Count)
            return false;

        foreach (var key in record1.Keys)
        {
            if (!record2.ContainsKey(key))
                return false;

            var value1 = NormalizeValue(record1[key]);
            var value2 = NormalizeValue(record2[key]);

            if (value1 != value2)
                return false;
        }

        return true;
    }

    private string NormalizeValue(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var numValue))
            return numValue.ToString(CultureInfo.InvariantCulture);

        if (DateTime.TryParse(value, out var dateValue)) return dateValue.ToString("yyyy-MM-dd HH:mm:ss");

        return value.Trim();
    }
}