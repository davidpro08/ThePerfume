using System;
using UnityEngine;

public class DistillerSaveManager : MonoBehaviour
{
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
    public static void Touch(GameSave save, string distillerID, DistillerSaveData snapshot)
    {
        int index = save.distillers.FindIndex(d => d.id == distillerID);
        if (index >= 0) save.distillers[index] = snapshot;
        else save.distillers.Add(snapshot);
    }

    public static long NowUnixMs()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
