using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BenchUIManager : MonoBehaviour
{
    public static BenchUIManager Instance { get; private set; }

    [Header("아이템 생성 설정")]
    [SerializeField] private Transform itemSpawnTray; // 아이템이 올라갈 철재 쟁반
    [SerializeField] private List<Transform> transformPos;

    [SerializeField] private ItemDataBase database;
    // 트레이 위에 생성된 아이템 리스트
    private List<GameObject> spawnedItemOnTray = new List<GameObject>();
    public List<int> spawnedItemData = new List<int>();
    public bool warningCanvasOpen = false;

    void Start()
    {
        GameSave save = SaveManager.Instance.CurrentSave ?? SaveManager.Load();
        if (save != null)
        {
            BenchLoad(save);
        }
    }
    void Awake()
    {
        Debug.Log($"Awake 실행");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ============================ 시스템 관련 코드 =====================================
    // tray에 아이템 랜덤 생성
    public void SpawnItemOnTray(ItemData itemToSpawn, int count)
    {
        Debug.Log($"SpawnItemOnTray. 아이템: {itemToSpawn?.name}, 수량: {count}");
        CropData cropData = itemToSpawn as CropData;
        if (itemToSpawn == null || cropData == null || cropData.itemOnTray == null || itemSpawnTray == null)
        {
            if (itemToSpawn == null) { Debug.Log("itemToSpawn=null"); return; }
            if (cropData == null) { Debug.Log($"CropData 형변환 실패"); return; }
            if (cropData.itemOnTray == null) { Debug.Log($"CropData.itemOnTray==null"); return; }
            if (itemSpawnTray == null) { Debug.Log($"itemSpawnTray==null"); return; }
        }

        int spawnd = 0;

        foreach (Transform spawnPoint in transformPos)
        {
            if (spawnPoint.childCount == 0)
            {
                GameObject spawndItem = Instantiate(cropData.itemOnTray, spawnPoint.position, Quaternion.identity);
                if (spawndItem == null)
                {
                    Debug.Log($"Instantiate 실패");
                    continue;
                }

                spawndItem.transform.SetParent(spawnPoint);
                spawnedItemOnTray.Add(spawndItem);

                ItemOnTrayClick trayClick = spawndItem.GetComponent<ItemOnTrayClick>();
                if (trayClick != null)
                {
                    trayClick.ItemData = itemToSpawn;
                    Debug.Log($"아이템 데이터 할당 완료: {itemToSpawn.id}");
                    spawnedItemData.Add(itemToSpawn.id);
                }
                else
                {
                    Debug.LogWarning($"{spawndItem.name}에 ItemOnTrayClick 컴포넌트가 없습니다.");
                }

                Debug.Log($"{spawndItem.name} {spawnd + 1}개 생성");
                spawnd++;
                // InventoryManager.Instance.RemoveItem(itemToSpawn, 1);

                if (spawnd >= count) break;
            }
        }
        if (spawnd < count)
        {
            Debug.Log("트레이 빈칸 부족");
        }
    }

    // Tray 아이템 삭제
    public void RemoveSpawnedItem(GameObject itemToRemove)
    {
        if (spawnedItemOnTray.Contains(itemToRemove))
        {
            ItemOnTrayClick trayClick = itemToRemove.GetComponent<ItemOnTrayClick>();
            if (trayClick != null)
            {
                int idToRemove = trayClick.ItemData.id;
                if (spawnedItemData.Contains(idToRemove))
                {
                    spawnedItemData.Remove(idToRemove);
                }
                else
                {
                    Debug.LogWarning($"spawnedItemData에 ID {idToRemove}가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"{itemToRemove.name}에 ItemOnTrayClick 컴포넌트가 없습니다.");
            }
            spawnedItemOnTray.Remove(itemToRemove);
            Destroy(itemToRemove);
        }
    }

    // 아이템 존재 여부 확인
    public bool HasSpawnedItemOnTray()
    {
        // 리스트가 비어있으면 false
        return spawnedItemOnTray != null && spawnedItemOnTray.Count > 0;
    }

    // ======================== save/load 관련 코드 =====================================
    public static void BenchSave(GameSave save)
    {
        BenchSaveData saveData = new BenchSaveData();
        saveData.spawnedItemData = new List<int>(Instance.spawnedItemData);
        save.bench = saveData;
    }

    public void BenchLoad(GameSave save)
    {
        if (save == null || save.bench == null)
        {
            Debug.Log("BenchLoad: save is null");
            return;
        }

        spawnedItemData = new List<int>(save.bench.spawnedItemData);

        // 기존에 생성된 아이템 삭제
        foreach (GameObject item in spawnedItemOnTray)
        {
            if (item != null) Destroy(item);
        }
        spawnedItemOnTray.Clear();

        List<int> loadList = new List<int>(save.bench.spawnedItemData);

        spawnedItemData.Clear();

        // 저장된 데이터 기반으로 아이템 생성
        foreach (int itemId in loadList)
        {
            ItemData itemData = null;
            if (database != null) itemData = database.GetItemByID(itemId);

            if (itemData != null)
            {
                SpawnItemOnTray(itemData, 1);
            }
            else
            {
                Debug.LogWarning($"BenchLoad: ItemData with ID {itemId} not found.");
            }
        }
    }
}
