using System;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class MixtureSaveData
{
    public int baseEssenceID;
    public int middleEssenceID;
    public int topEssenceID;

    public bool perfumeComplete;
    public int perfumeID;

    public float colorR;
    public float colorG;
    public float colorB;

    public float warm;
    public float cool;
    public float relax;
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
