using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor;

// ========== 저장용 데이터 스키마 ============
[Serializable]
public class PetalSlotData
{
    public int index;
    public int itemID;
}

[Serializable]
public class DistillerSaveData
{
    public string id;
    public List<int> occupiedFuelSlots = new List<int>();
    public List<PetalSlotData> petalSlots = new List<PetalSlotData>();
    public bool isMaking;
    public long craftStartUtcMs;
    public int craftDurationMs;
    public int essenceid;
    public bool essenceReady;
}

[Serializable]
public class GameSave
{
    public List<DistillerSaveData> distillers = new List<DistillerSaveData>();
    public long lastSavedUtc;
}


// ================ 저장 / 로드 로직 ===================
public static class TillSaveService
{
    private const string FileName = "distillers.json";
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static long NowUnixMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static GameSave Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                GameSave fresh = new GameSave { lastSavedUtc = NowUnixMs() };
                Save(fresh);
                return fresh;
            }

            String json = File.ReadAllText(FilePath, Encoding.UTF8);
            GameSave save = JsonUtility.FromJson<GameSave>(json);
            return save ?? new GameSave { lastSavedUtc = NowUnixMs() };
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSercive.Load 실패: {e.Message}");
            return new GameSave { lastSavedUtc = NowUnixMs() };
        }
    }

    public static void Save(GameSave save)
    {
        try
        {
            save.lastSavedUtc = NowUnixMs();
            String json = JsonUtility.ToJson(save, prettyPrint: true);
            File.WriteAllText(FilePath, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveService.Save 실패: {e.Message}");
        }
    }

    // ================ 보조 로직 =================
    // 리스트에서 ID로 찾아오기 / 없으면 생성
    public static DistillerSaveData GetOrCreate(GameSave save, string distillerID)
    {
        DistillerSaveData data = save.distillers.Find(d => d.id == distillerID);
        if (data != null) return data;

        data = new DistillerSaveData { id = distillerID };
        save.distillers.Add(data);
        return data;
    }

    // 스냅샷으로 교체/업서트/저장
    public static GameSave Touch(string distillerID, DistillerSaveData snapshot)
    {
        GameSave save = Load();
        int exist = save.distillers.FindIndex(d => d.id == distillerID);
        if (exist >= 0) save.distillers[exist] = snapshot;
        else save.distillers.Add(snapshot);
        Save(save);
        return save;
    }
}


