using System;
using System.IO;
using System.Text;
using UnityEngine;

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

public static class MIxtureServerSave
{
    private const string FileName = "mixture.json";
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static MixtureSaveData Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new MixtureSaveData();

            string json = File.ReadAllText(FilePath, Encoding.UTF8);
            return JsonUtility.FromJson<MixtureSaveData>(json) ?? new MixtureSaveData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"MixtureSaveServer.Load 실패 : {e.Message}");
            return new MixtureSaveData();
        }
    }

    public static void Save(MixtureSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"MixtureSaveServer.Save 실패 : {e.Message}");
        }
    }
}
