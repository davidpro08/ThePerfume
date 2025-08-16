using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

public static class CSVParser
{
    /// <summary>
    /// CSV 파일을 Resources 폴더에서 로드하여 파싱합니다.
    /// </summary>
    /// <param name="resourcePath">Resources 폴더 내의 경로 (확장자 제외)</param>
    /// <returns>파싱된 CSV 데이터</returns>
    public static List<Dictionary<string, string>> Parse(string resourcePath)
    {
        TextAsset data = Resources.Load<TextAsset>(resourcePath);
        if (data == null)
        {
            Debug.LogError($"CSV file not found at: Resources/{resourcePath}");
            return new List<Dictionary<string, string>>();
        }
        return ParseFromText(data.text);
    }

    /// <summary>
    /// CSV 텍스트를 직접 파싱합니다.
    /// </summary>
    /// <param name="csvText">CSV 형식의 텍스트</param>
    /// <returns>파싱된 CSV 데이터</returns>
    public static List<Dictionary<string, string>> ParseFromText(string csvText)
    {
        var list = new List<Dictionary<string, string>>();
        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogWarning("CSV 텍스트가 비어있습니다.");
            return list;
        }

        try
        {
            // 줄 단위 분리 (따옴표 안의 줄바꿈도 처리)
            var rows = SplitCsvLines(csvText);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("CSV에 헤더만 있거나 데이터가 없습니다.");
                return list;
            }

            var header = ParseCsvRow(rows[0]);
            if (header.Count > 0)
            {
                header[0] = header[0].TrimStart('\uFEFF'); // BOM 제거
            }

            // 헤더 검증
            if (header.Count == 0)
            {
                Debug.LogError("CSV 헤더를 파싱할 수 없습니다.");
                return list;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                var values = ParseCsvRow(rows[i]);
                if (values.Count == 0) continue;

                var entry = new Dictionary<string, string>();
                for (int j = 0; j < header.Count && j < values.Count; j++)
                {
                    entry[header[j]] = values[j];
                }

                // 헤더보다 값이 적은 경우 빈 문자열로 채움
                for (int j = values.Count; j < header.Count; j++)
                {
                    entry[header[j]] = "";
                }

                list.Add(entry);
            }

            Debug.Log($"CSV 파싱 완료: {list.Count}개 행, {header.Count}개 컬럼");
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV 파싱 중 오류 발생: {e.Message}");
        }

        return list;
    }

    /// <summary>
    /// CSV 텍스트를 Dictionary<string, object> 형태로 파싱합니다.
    /// </summary>
    /// <param name="csvText">CSV 형식의 텍스트</param>
    /// <returns>파싱된 CSV 데이터 (object 타입)</returns>
    public static List<Dictionary<string, object>> ParseFromTextAsObject(string csvText)
    {
        var stringData = ParseFromText(csvText);
        var objectData = new List<Dictionary<string, object>>();

        foreach (var row in stringData)
        {
            var objectRow = new Dictionary<string, object>();
            foreach (var kvp in row)
            {
                objectRow[kvp.Key] = kvp.Value;
            }
            objectData.Add(objectRow);
        }

        return objectData;
    }

    /// <summary>
    /// CSV 텍스트를 Dictionary<string, object> 형태로 파싱합니다. (대소문자 구분 없음)
    /// </summary>
    /// <param name="csvText">CSV 형식의 텍스트</param>
    /// <param name="caseInsensitive">헤더 매칭 시 대소문자 구분 여부 (true: 구분 없음, false: 구분함)</param>
    /// <returns>파싱된 CSV 데이터 (object 타입)</returns>
    public static List<Dictionary<string, object>> ParseFromTextAsObject(string csvText, bool caseInsensitive)
    {
        if (!caseInsensitive)
        {
            return ParseFromTextAsObject(csvText);
        }

        var list = new List<Dictionary<string, object>>();
        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogWarning("CSV 텍스트가 비어있습니다.");
            return list;
        }

        try
        {
            // 줄 단위 분리 (따옴표 안의 줄바꿈도 처리)
            var rows = SplitCsvLines(csvText);
            if (rows.Count <= 1)
            {
                Debug.LogWarning("CSV에 헤더만 있거나 데이터가 없습니다.");
                return list;
            }

            var header = ParseCsvRow(rows[0]);
            if (header.Count > 0)
            {
                header[0] = header[0].TrimStart('\uFEFF'); // BOM 제거
            }

            // 헤더 검증
            if (header.Count == 0)
            {
                Debug.LogError("CSV 헤더를 파싱할 수 없습니다.");
                return list;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                var values = ParseCsvRow(rows[i]);
                if (values.Count == 0) continue;

                var entry = new Dictionary<string, object>();
                for (int j = 0; j < header.Count && j < values.Count; j++)
                {
                    entry[header[j]] = values[j];
                }

                // 헤더보다 값이 적은 경우 빈 문자열로 채움
                for (int j = values.Count; j < header.Count; j++)
                {
                    entry[header[j]] = "";
                }

                list.Add(entry);
            }

            Debug.Log($"CSV 파싱 완료 (대소문자 구분 없음): {list.Count}개 행, {header.Count}개 컬럼");
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV 파싱 중 오류 발생: {e.Message}");
        }

        return list;
    }

    /// <summary>
    /// CSV 파일을 Resources 폴더에서 로드하여 object 타입으로 파싱합니다.
    /// </summary>
    /// <param name="resourcePath">Resources 폴더 내의 경로 (확장자 제외)</param>
    /// <returns>파싱된 CSV 데이터 (object 타입)</returns>
    public static List<Dictionary<string, object>> ParseAsObject(string resourcePath)
    {
        var stringData = Parse(resourcePath);
        var objectData = new List<Dictionary<string, object>>();

        foreach (var row in stringData)
        {
            var objectRow = new Dictionary<string, object>();
            foreach (var kvp in row)
            {
                objectRow[kvp.Key] = kvp.Value;
            }
            objectData.Add(objectRow);
        }

        return objectData;
    }

    /// <summary>
    /// 한 줄을 CSV 규칙에 맞게 파싱합니다.
    /// </summary>
    private static List<string> ParseCsvRow(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // "" → " 로 치환 (이스케이프된 따옴표)
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes; // 따옴표 열기/닫기
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        // 마지막 필드 추가
        result.Add(sb.ToString().Trim());
        return result;
    }

    /// <summary>
    /// 따옴표 안의 줄바꿈까지 고려해서 CSV를 라인 단위로 나눕니다.
    /// </summary>
    private static List<string> SplitCsvLines(string text)
    {
        var lines = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in text)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                sb.Append(c);
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (sb.Length > 0)
                {
                    lines.Add(sb.ToString().TrimEnd('\r', '\n'));
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0)
            lines.Add(sb.ToString().TrimEnd('\r', '\n'));

        return lines;
    }

    /// <summary>
    /// CSV 데이터를 특정 컬럼으로 정렬합니다.
    /// </summary>
    /// <param name="data">정렬할 데이터</param>
    /// <param name="columnName">정렬 기준 컬럼</param>
    /// <param name="ascending">오름차순 여부</param>
    public static void SortByColumn(List<Dictionary<string, string>> data, string columnName, bool ascending = true)
    {
        if (data == null || data.Count == 0) return;

        data.Sort((a, b) =>
        {
            if (!a.ContainsKey(columnName) || !b.ContainsKey(columnName))
                return 0;

            string aVal = a[columnName];
            string bVal = b[columnName];

            if (int.TryParse(aVal, out int aInt) && int.TryParse(bVal, out int bInt))
            {
                return ascending ? aInt.CompareTo(bInt) : bInt.CompareTo(aInt);
            }

            return ascending ? string.Compare(aVal, bVal) : string.Compare(bVal, aVal);
        });
    }

    /// <summary>
    /// 대소문자 구분 없이 키를 찾습니다.
    /// </summary>
    /// <param name="dictionary">검색할 딕셔너리</param>
    /// <param name="key">찾을 키</param>
    /// <returns>찾은 값 또는 null</returns>
    public static object GetValueCaseInsensitive(Dictionary<string, object> dictionary, string key)
    {
        if (dictionary == null || string.IsNullOrEmpty(key)) return null;

        // 정확한 키로 먼저 찾기
        if (dictionary.ContainsKey(key))
            return dictionary[key];

        // 대소문자 구분 없이 찾기
        foreach (var kvp in dictionary)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return null;
    }

    /// <summary>
    /// 대소문자 구분 없이 키가 존재하는지 확인합니다.
    /// </summary>
    /// <param name="dictionary">검색할 딕셔너리</param>
    /// <param name="key">찾을 키</param>
    /// <returns>키 존재 여부</returns>
    public static bool ContainsKeyCaseInsensitive(Dictionary<string, object> dictionary, string key)
    {
        if (dictionary == null || string.IsNullOrEmpty(key)) return false;

        // 정확한 키로 먼저 찾기
        if (dictionary.ContainsKey(key))
            return true;

        // 대소문자 구분 없이 찾기
        foreach (var kvp in dictionary)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// CSV 데이터를 검증합니다. (대소문자 구분 없음)
    /// </summary>
    /// <param name="data">검증할 CSV 데이터</param>
    /// <param name="requiredColumns">필수 컬럼 목록</param>
    /// <param name="caseInsensitive">대소문자 구분 여부</param>
    /// <returns>검증 결과</returns>
    public static bool ValidateData(List<Dictionary<string, object>> data, string[] requiredColumns, bool caseInsensitive = false)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogWarning("검증할 데이터가 없습니다.");
            return false;
        }

        if (requiredColumns == null || requiredColumns.Length == 0)
        {
            Debug.LogWarning("필수 컬럼이 정의되지 않았습니다.");
            return false;
        }

        var firstRow = data[0];
        foreach (var column in requiredColumns)
        {
            bool hasColumn = caseInsensitive ?
                ContainsKeyCaseInsensitive(firstRow, column) :
                firstRow.ContainsKey(column);

            if (!hasColumn)
            {
                Debug.LogError($"필수 컬럼 '{column}'이(가) 없습니다.");
                return false;
            }
        }

        return true;
    }
}
