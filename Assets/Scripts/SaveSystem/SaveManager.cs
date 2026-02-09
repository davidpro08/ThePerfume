using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class GameSave
{
    public List<InstallationSaveData> installationList = new();
    public List<FarmSaveData> farms = new();
    public BenchSaveData bench = new();
    public List<DistillerSaveData> distillers = new List<DistillerSaveData>(); // ID 기반 다중 소환
    public MixtureSaveData mixture = new(); // 단일 소환
    public List<InventoryItemSaveData> inventory = new();
    public TutorialSaveData tutorial = new();
    public StorySaveData story = new();
    public long totalPlayTime;
}

// ============= 저장 데이터 스키마 =============

[Serializable]
public class InventoryItemSaveData
{
    public int itemID;
    public int quantity;
    public int location;
}

[Serializable]
public class InstallationSaveData
{
    public int itemID;
    public Vector3Int tilePosition;
    public string sceneName;
    // Distiller를 위한 distiller 고유 ID도 붙여야하나..?
}

[Serializable]
public class FarmSaveData
{
    public Vector3Int gridPosition;
    public bool isOccpuied;
    public bool isWatered;
    public int seedItemID;
    public int growthStage;
    public float plantedTimeAtTotalPlayTime;
}

[Serializable]
public class BenchSaveData
{
    public List<int> spawnedItemData = new List<int>();
}

[Serializable]
public class MixtureSaveData
{
    public int baseID, middleID, topID;
    public int pBaseID, pMiddleID, pTopID;

    public bool perfumeComplete;
    public int perfumeID;

    public float colorR, colorG, colorB;

    public float warm, cool, relax;

    public bool baseOn, middleOn, topOn;
    public bool pBaseOn, pMiddleOn, pTopOn;
    public bool punnelOn = true;
}

[Serializable]
public class DistillerSaveData
{
    public string id; // Distiller 고유 ID, 아이템 ID 아님
    public List<bool> occupiedFuelSlots = new List<bool>() { false, false, false };
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

[Serializable]
public class StorySaveData
{
    public bool isPrologueCompleted = false;
    public int lastDialougeIndex = 0;
    public bool isStoryMode = true;
    public bool isChapter1Done = false;
}

[Serializable]
public class TutorialSaveData
{
    public bool isTutorialEnd = false; // true가 끝
    public string currentStep = ""; // narration_nnn_nnn, 마지막으로 완료된 대화 ID
    public HashSet<string> completedStepNames = new(); // 완료된 튜토리얼 단계의 이름들
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public GameSave CurrentSave { get; private set; }
    [Header("DB 참조")]
    public ItemDataBase itemDataBase;
    private Dictionary<int, ItemData> itemDict;

    // ============ Json > Game Save / Game > Json Load ============
    private const string FileName = "ThePerfumeSaveFile.json";
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (CurrentSave == null)
            CurrentSave = Load();

        itemDict = new Dictionary<int, ItemData>();
        foreach (var item in itemDataBase.items)
        {
            itemDict[item.id] = item;
        }
    }
    void Update()
    {
        CurrentSave.totalPlayTime += (long)(Time.deltaTime * 1000);
    }

    public static GameSave Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                GameSave newSave = new GameSave();
                Save(newSave);
                return newSave;
            }

            string json = File.ReadAllText(FilePath, Encoding.UTF8);
            return JsonUtility.FromJson<GameSave>(json) ?? new GameSave();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"MixtureSaveServer.Load 실패 : {e.Message}");
            return new GameSave();
        }
    }

    public static void Save(GameSave data)
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

    public void ResetGame()
    {
        CurrentSave = new GameSave();
        Save(CurrentSave);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ResetInventory();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ResetTutorial();
    }

    // ========== 모든 SaveManager 통합 =========
    [SerializeField] private BuildController buildController;

    public void SaveGame()
    {
        GameSave save = CurrentSave;
        InstallatioinSaveManager.SaveInstallations(save, buildController);
        FarmSaveManager.SaveFarms(save, buildController);
        TutorialManager.Instance?.PrepareSaveData(save);

        SaveManager.Save(save);
    }

    public void LoadGame()
    {
        GameSave save = SaveManager.Load();
        if (buildController != null)
            InstallatioinSaveManager.LoadInstallations(save, buildController);
        FarmSaveManager.LoadFarms(save, buildController);
    }

    // ======== 보조 함수 =========
    public void SetBuildController(BuildController controller)
    {
        buildController = controller;
    }

    public ItemData GetItemData(int id)
    {
        itemDict.TryGetValue(id, out var item);
        return item;
    }
}
