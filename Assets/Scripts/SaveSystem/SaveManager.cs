using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class GameSave
{
    public List<FarmSaveData> farms = new();
    public List<DistillerSaveData> distillers = new List<DistillerSaveData>();
    public MixtureSaveData mixture = new();
    public List<InventoryItemSaveData> inventory = new();
    public long lastSavedUtc;
}

// ============= 저장 데이터 스키마 =============
[Serializable]
public class InventoryItemSaveData
{
    public int itemID;
    public int quantity;
}

[Serializable]
public class FarmSaveData
{
    public Vector3Int tilePosition;
    public bool isOccpuied;
    public bool isWatered;
    public int cropID;
    public int growthStage;
    public long plantedTime;
}

[Serializable]
public class MixtureSaveData
{
    public int baseEssenceID, middleEssenceID, topEssenceID;

    public bool perfumeComplete;
    public int perfumeID;

    public float colorR, colorG, colorB;

    public float warm, cool, relax;

    public bool baseOn, middleOn, topOn;
    public bool pBaseOn, pMiddleOn, pTopOn;
    public bool punnelOn;
}

[Serializable]
public class DistillerSaveData
{
    Vector3Int tilePosition;
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
public class PetalSlotData
{
    public int index;
    public int itemID;
}
// =================== save / load ===================

public static class SaveManager
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "thePerfumeSaveData.json");
    public static long NowUnixMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static GameSave Load()
    {
        if (!File.Exists(FilePath))
            return new GameSave();

        string json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<GameSave>(json) ?? new GameSave { lastSavedUtc = NowUnixMs() };
    }

    public static void Save(GameSave save)
    {
        try
        {
            save.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string json = JsonUtility.ToJson(save, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager.Save] fail: {e.Message}");
        }
    }

    public static void TouchDistiller(string distillerID, DistillerSaveData snapshot)
    {
        GameSave save = Load();
        int index = save.distillers.FindIndex(d => d.id == distillerID);
        if (index >= 0) save.distillers[index] = snapshot;
        else save.distillers.Add(snapshot);
        Save(save);
    }

    public static void Touch()
    {
        GameSave save = Load();
        save.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Save(save);
    }
}