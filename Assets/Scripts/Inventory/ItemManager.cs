
using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("CSV 설정")]
    [Tooltip("인스펙터에서 아이템 데이터 CSV 파일을 여기에 할당하세요.")]
    public TextAsset itemCsvFile;

    [Header("데이터베이스 (인스펙터 확인용)")]
    [Tooltip("게임 실행 중 CSV에서 로드된 아이템 목록입니다.")]
    [SerializeField] private List<ItemData> itemDataForInspector = new List<ItemData>();

    // 빠른 조회를 위한 딕셔너리 (런타임용)
    private readonly Dictionary<int, ItemData> itemDatabase = new Dictionary<int, ItemData>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadItemData();
    }

    private void LoadItemData()
    {
        if (itemCsvFile == null)
        {
            Debug.LogError("Item CSV 파일이 할당되지 않았습니다!");
            return;
        }

        try
        {
            // 데이터 로드 전, 기존 데이터 초기화
            itemDatabase.Clear();

            // 새로운 CSVParser의 object 타입 파싱 메서드 사용 (대소문자 구분 없음)
            var parsedData = CSVParser.ParseFromTextAsObject(itemCsvFile.text, true);

            // 데이터 검증 (대소문자 구분 없음)
            if (!ValidateItemData(parsedData))
            {
                Debug.LogError("아이템 CSV 데이터 검증에 실패했습니다.");
                return;
            }

            foreach (var row in parsedData)
            {
                int id = GetInt(row, "id");
                if (id == 0) continue; // Skip rows with invalid ID

                ItemType itemType = GetEnum<ItemType>(row, "itemType");

                ItemData itemData = CreateItemData(itemType);
                if (itemData == null) continue;

                PopulateBaseItemData(itemData, row, id, itemType);
                PopulateSpecificItemData(itemData, row);

                if (!itemDatabase.ContainsKey(id))
                {
                    itemDatabase.Add(id, itemData);
                }
                else
                {
                    Debug.LogWarning($"Item with ID {id} already exists in the database. Skipping duplicate.");
                }
            }

            // 인스펙터 표시용 리스트 업데이트
            UpdateItemDataForInspector();

            Debug.Log($"아이템 데이터 로드 완료: {itemDatabase.Count}개 아이템");
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 데이터 로드 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 아이템 CSV 데이터의 유효성을 검증합니다.
    /// </summary>
    private bool ValidateItemData(List<Dictionary<string, object>> parsedData)
    {
        if (parsedData == null || parsedData.Count == 0)
        {
            Debug.LogWarning("파싱된 아이템 데이터가 없습니다.");
            return false;
        }

        // 필수 컬럼 검증 (대소문자 구분 없음)
        string[] requiredColumns = { "id", "itemName", "itemType" };
        var firstRow = parsedData[0];
        foreach (var column in requiredColumns)
        {
            if (!CSVParser.ContainsKeyCaseInsensitive(firstRow, column))
            {
                Debug.LogError($"필수 컬럼 '{column}'이(가) 아이템 CSV에 없습니다.");
                return false;
            }
        }

        // 데이터 행 검증
        int validRows = 0;
        for (int i = 0; i < parsedData.Count; i++)
        {
            var row = parsedData[i];
            if (CSVParser.ContainsKeyCaseInsensitive(row, "id") && GetInt(row, "id") > 0)
            {
                validRows++;
            }
            else
            {
                Debug.LogWarning($"행 {i + 1}: 유효하지 않은 ID");
            }
        }

        if (validRows == 0)
        {
            Debug.LogError("유효한 아이템 데이터가 없습니다.");
            return false;
        }

        Debug.Log($"아이템 데이터 검증 완료: {validRows}개 유효한 행");
        return true;
    }

    /// <summary>
    /// 데이터베이스의 모든 아이템을 인스펙터 확인용 리스트에 복사합니다.
    /// </summary>
    private void UpdateItemDataForInspector()
    {
        itemDataForInspector.Clear();
        foreach (var itemData in itemDatabase.Values)
        {
            itemDataForInspector.Add(itemData);
        }
    }

    private ItemData CreateItemData(ItemType type)
    {
        switch (type)
        {
            case ItemType.Tool:
                return ScriptableObject.CreateInstance<ToolData>();
            case ItemType.Seed:
                return ScriptableObject.CreateInstance<SeedData>();
            case ItemType.Crop:
                return ScriptableObject.CreateInstance<CropData>();
            case ItemType.Essence:
                return ScriptableObject.CreateInstance<EssenceData>();
            case ItemType.Perfume:
                return ScriptableObject.CreateInstance<PerfumeData>();
            case ItemType.Etc:
            case ItemType.Material:
                return ScriptableObject.CreateInstance<MaterialData>();
            default:
                Debug.LogWarning($"No ItemData class found for type: {type}");
                return null;
        }
    }

    private void PopulateBaseItemData(ItemData data, Dictionary<string, object> row, int id, ItemType itemType)
    {
        data.id = id;
        data.itemName = GetString(row, "itemName");
        data.description = GetString(row, "description");
        data.itemType = itemType;
        data.isStackable = GetBool(row, "isStackable");
        data.maxStack = GetInt(row, "maxStack");
        data.nowStack = GetInt(row, "nowStack");
        data.isTradable = GetBool(row, "isTradeable");
        data.buyPrice = GetInt(row, "buyPrice");
        data.sellPrice = GetInt(row, "sellPrice");
    }

    private void PopulateSpecificItemData(ItemData data, Dictionary<string, object> row)
    {
        if (data is ToolData toolData)
        {
            toolData.toolType = GetEnum<ToolType>(row, "toolType");
            toolData.maxDurability = GetInt(row, "maxDurability");
            toolData.nowDurability = GetInt(row, "nowDurability");
        }
        else if (data is SeedData seedData)
        {
            seedData.seedType = GetEnum<SeedType>(row, "seedType");
            seedData.growIntoCropType = GetEnum<CropType>(row, "cropType");
        }
        else if (data is CropData cropData)
        {
            cropData.cropType = GetEnum<CropType>(row, "cropType");
        }
        else if (data is EssenceData essenceData)
        {
            essenceData.essenceType = GetEnum<EssenceType>(row, "essenceType");
            essenceData.essenceWarm = GetInt(row, "essenceWarm");
            essenceData.essenceCool = GetInt(row, "essenceCool");
            essenceData.essenceRelax = GetInt(row, "essenceRelax");
        }
        else if (data is PerfumeData perfumeData)
        {
            perfumeData.perfumeType = GetEnum<PerfumeType>(row, "perfumeType");
            perfumeData.perfumeWarm = GetFloat(row, "perfumeWarm");
            perfumeData.perfumeCool = GetFloat(row, "perfumeCool");
            perfumeData.perfumeRelax = GetFloat(row, "perfumeRelax");
        }
    }

    public ItemData GetItemData(int id)
    {
        if (itemDatabase.TryGetValue(id, out ItemData data))
        {
            // return Instantiate(data);
            return data;
        }
        Debug.LogWarning($"Item with ID {id} not found in the database.");
        return null;
    }

    #region Helper Methods

    private string GetString(Dictionary<string, object> dict, string key)
    {
        var value = CSVParser.GetValueCaseInsensitive(dict, key);
        return value?.ToString() ?? "";
    }

    private int GetInt(Dictionary<string, object> dict, string key)
    {
        var value = CSVParser.GetValueCaseInsensitive(dict, key);
        if (value != null && int.TryParse(value.ToString(), out int result))
        {
            return result;
        }
        return 0;
    }

    private float GetFloat(Dictionary<string, object> dict, string key)
    {
        var value = CSVParser.GetValueCaseInsensitive(dict, key);
        if (value != null && float.TryParse(value.ToString(), out float result))
        {
            return result;
        }
        return 0f;
    }

    private bool GetBool(Dictionary<string, object> dict, string key)
    {
        var value = CSVParser.GetValueCaseInsensitive(dict, key);
        if (value != null)
        {
            string val = value.ToString();
            return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private T GetEnum<T>(Dictionary<string, object> dict, string key) where T : struct
    {
        var value = CSVParser.GetValueCaseInsensitive(dict, key);
        if (value != null)
        {
            string valueStr = value.ToString();
            if (!string.IsNullOrEmpty(valueStr) && Enum.TryParse<T>(valueStr, true, out T result))
            {
                return result;
            }
        }
        return default(T);
    }

    #endregion
}
