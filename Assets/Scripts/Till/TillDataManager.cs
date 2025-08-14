using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;


// 모든 증류기의 데이터 관리 >> 
// 새로운 스크립트로 프리팹에 tiller.cs 붙여서 고유ID로 각 상태 확인해야하나

public class TillDataManager : MonoBehaviour
{
    public static TillDataManager Instance { get; private set; }
    // 모든 증류기
    private Dictionary<string, DistillerState> allDistillerState = new Dictionary<string, DistillerState>();
    // 실제 씬에 있는 증류기
    private Dictionary<string, Distiller> currentDistiller = new Dictionary<string, Distiller>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllDistillerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterDistiller(Distiller distiller)
    {
        if (!currentDistiller.ContainsKey(distiller.distillerID))
        {
            currentDistiller.Add(distiller.distillerID, distiller);
        }
        if (!allDistillerState.ContainsKey(distiller.distillerID))
        {
            allDistillerState.Add(distiller.distillerID, new DistillerState { distillerID = distiller.distillerID });
        }
    }

    public DistillerState GetDistillerState(string distillerID)
    {
        if (allDistillerState.ContainsKey(distillerID))
        {
            return allDistillerState[distillerID];
        }
        return null;
    }

    public void UpdateDistillerState(string distillerID, DistillerState distillerState)
    {
        if (allDistillerState.ContainsKey(distillerID))
        {
            allDistillerState[distillerID] = distillerState;
            Debug.Log($"증류기 {distillerID} 상태 업데이트");
            SaveAllDistillerData();
        }
    }

    private void SaveAllDistillerData()
    {
        DistillerStateDataWrapper wrapper = new DistillerStateDataWrapper(allDistillerState);
        string jsonData = JsonUtility.ToJson(wrapper);
        // 실제 파일 저장 러트
        string filePath = Application.persistentDataPath + "/distillerData.json";
        Debug.Log("증류기 데이터 저장 " + filePath);
    }

    void LoadAllDistillerData()
    {
        string filePath = Application.persistentDataPath + "/distillerData.json";
        if (System.IO.File.Exists(filePath))
        {
            string jsonData = System.IO.File.ReadAllText(filePath);
            DistillerStateDataWrapper wrapper = JsonUtility.FromJson<DistillerStateDataWrapper>(jsonData);
            allDistillerState = wrapper.toDictionary();
            Debug.Log("증류기 데이터 로드 " + filePath);
        }
        else
        {
            allDistillerState = new Dictionary<string, DistillerState>();
            Debug.Log("증류기 데이터 파일 새로 생성");
        }
    }

    // 혹시 몰라서 삭제하는 로직까지 > 파괴하거나 그러면 삭제될 듯
    public void RemoveDistiller(string distillerID)
    {
        if (allDistillerState.ContainsKey(distillerID))
        {
            allDistillerState.Remove(distillerID);
            currentDistiller.Remove(distillerID);
            SaveAllDistillerData();
        }
    }
}
