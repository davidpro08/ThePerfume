using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor;


// ================ 저장 / 로드 로직 ===================
public class SaveService : MonoBehaviour
{
    public static SaveService Instance { get; private set; }
    static string filePath => Path.Combine(Application.persistentDataPath, "thePerfumeSaveData.json");

    public static GameSave Load()
    {
        if (!File.Exists(filePath)) return new GameSave { lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<GameSave>(json);
    }

    public static void Save(GameSave save)
    {
        try
        {
            save.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            String json = JsonUtility.ToJson(save, prettyPrint: true);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveService.Save 실패: {e.Message}");
        }
    }

    // ================ 보조 로직 =================

    // 스냅샷으로 교체/업서트/저장
    public static void Touch()
    {
        GameSave save = Load();
        save.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Save(save);
    }
}


