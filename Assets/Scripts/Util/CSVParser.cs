using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class CSVParser
{
    // Parse from a file path in the Resources folder
    public static List<Dictionary<string, object>> Parse(string resourcePath)
    {
        TextAsset data = Resources.Load<TextAsset>(resourcePath);
        if (data == null)
        {
            Debug.LogError($"CSV file not found at: Resources/{resourcePath}");
            return new List<Dictionary<string, object>>();
        }
        return ParseFromText(data.text);
    }

    // Parse from raw CSV text content
    public static List<Dictionary<string, object>> ParseFromText(string csvText)
    {
        var list = new List<Dictionary<string, object>>();
        if (string.IsNullOrEmpty(csvText)) return list;

        var lines = Regex.Split(csvText, "\r\n|\n|\r");

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], ",");
        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], ",");
            if (values.Length == 0 || string.IsNullOrEmpty(values[0])) continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                entry[header[j]] = value;
            }
            list.Add(entry);
        }
        return list;
    }
}