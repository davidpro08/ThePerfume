using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BenchUIManager : MonoBehaviour
{
    public static BenchUIManager Instance { get; private set; }

    [Header("아이템 생성 설정")]
    [SerializeField] private Transform itemSpawnTray; // 아이템이 올라갈 철재 쟁반
    [SerializeField] private List<Transform> transformPos;
    // 트레이 위에 생성된 아이템 리스트
    private List<GameObject> spawnedItemOnTray = new List<GameObject>();
    public bool warningCanvasOpen = false;


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

                Debug.Log($"{spawndItem.name} {spawnd + 1}개 생성");
                spawnd++;
                InventoryManager.Instance.RemoveItem(itemToSpawn, 1);

                if (spawnd >= count) break;
            }
        }
        if (spawnd < count)
        {
            Debug.Log("트레이 빈칸 부족");
        }
    }

    // Tray 아이템 삭제
    public void RemoveSpawnedItemd(GameObject itemToRemove)
    {
        if (spawnedItemOnTray.Contains(itemToRemove))
        {
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
}
