using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem.Interactions;

public class Distiller : MonoBehaviour
{
    [Header("Identity/DB")]
    [SerializeField] string distillerID;
    [SerializeField] ItemDataBase dataBase;

    [Header("Slots")]
    [SerializeField] List<Transform> fuelSlotParent;
    [SerializeField] List<Transform> petalSlotParent;
    [SerializeField] Transform essenceTransform;

    [Header("Craft")]
    [SerializeField] int craftDurationSec = 1;
    //임시 아이템 아이디 정해지면 정리해야함
    [SerializeField] int fuelID = 99;

    bool isMaking;
    long craftStartUtcMs;
    int currentEssenceID;
    GameObject spawnedEssence;
    readonly List<GameObject> spawndItems = new List<GameObject>();

    void Start()
    {
        if (string.IsNullOrEmpty(distillerID) && GameContext.Instance != null)
        {
            distillerID = GameContext.Instance.SelectedDistillerID;
        }
        GameSave save = TillSaveService.Load();
        DistillerSaveData data = TillSaveService.GetOrCreate(save, distillerID);
        RebuildFromSave(data);

        if (isMaking) StartCatchupOrFinish();
    }

    // ================ 클릭 관련 ================

    public void PlaceFuel()
    {
        var slot = FindFirstEmpty(fuelSlotParent);
        if (slot == null)
        {
            TillUIManager.Instance.ShowWarningCanvas("no empty fuel slot");
            return;
        }

        if (!TryConsumeInventoryFuel(out FuelData fuelData))
        {
            TillUIManager.Instance.ShowWarningCanvas("need fuel item");
            return;
        }

        int slotIndex = slot.GetSiblingIndex();
        if (slotIndex >= fuelData.itemPrefabs.Count) return;

        FuelSpawnToSlot(fuelData, slotIndex, slot);
        TillSaveService.Touch(distillerID, SaveSnapshot());
        TryStartCraft();
    }

    public void PlacePetal(MaterialData petalData)
    {
        if (spawnedEssence != null)
        {
            EssenceData essence = dataBase?.ResolveEssence(currentEssenceID);
            if (essence != null)
            {
                SpriteRenderer sr = spawnedEssence.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == essence.essenceStage.progressStage[^1])
                {

                    TillUIManager.Instance.ShowWarningCanvas("Essence is already");
                    return;
                }
            }

        }

        if (petalData == null || !petalData.itemPrefab)
        {
            TillUIManager.Instance.ShowWarningCanvas("need petal item");
            return;
        }

        var slot = FindFirstEmpty(petalSlotParent);
        if (slot == null)
        {
            TillUIManager.Instance.ShowWarningCanvas("no empty petal slot");
            return;
        }
        if (!InventoryManager.Instance.RemoveItem(petalData, 1))
        {
            TillUIManager.Instance.ShowWarningCanvas("need petal item");
            return;
        }


        PetalSpawnToSlot(petalData, slot);
        TillSaveService.Touch(distillerID, SaveSnapshot());
        TryStartCraft();
    }

    public void OnEssenceClicked()
    {
        if (isMaking || spawnedEssence == null || currentEssenceID == 0) return; //  || !currentEssenceID.HasValue

        EssenceData essence = dataBase?.ResolveEssence(currentEssenceID);
        if (essence == null)
        {
            Debug.Log("Essence id invalid");
            return;
        }

        InventoryManager.Instance.AddItem(essence, 1);

        Destroy(spawnedEssence);
        spawnedEssence = null;
        currentEssenceID = 0;
        isMaking = false;

        TillSaveService.Touch(distillerID, SaveSnapshot());
    }

    // =========== 제작 로직 ==========

    void TryStartCraft()
    {
        if (isMaking) return;

        bool hasFuel = HasAtLeastOneFuel();
        MaterialData petal = FindFirstPetalMaterial();

        if (!hasFuel || petal == null || petal.essenceData == null) return;

        isMaking = true;
        craftStartUtcMs = TillSaveService.NowUnixMs();
        currentEssenceID = petal.essenceData.id;

        EssenceData essence = dataBase?.ResolveEssence(currentEssenceID);
        if (essence != null && essence.essenceStage != null)
        {
            if (spawnedEssence != null) Destroy(spawnedEssence);

            spawnedEssence = new GameObject("EssenceProgress");
            spawnedEssence.transform.SetParent(essenceTransform, false);
            spawnedEssence.transform.localPosition = Vector3.zero;

            var sr = spawnedEssence.AddComponent<SpriteRenderer>();
            sr.sprite = essence.essenceStage.progressStage[0];
            sr.color = essence.color;
            sr.sortingOrder = 10;
        }

        TillSaveService.Touch(distillerID, SaveSnapshot());
        StartCatchupOrFinish();
    }

    void StartCatchupOrFinish()
    {
        int craftDurationMs = craftDurationSec * 1000;
        long elapsed = TillSaveService.NowUnixMs() - craftStartUtcMs;
        if (elapsed < 0) elapsed = 0;
        if (elapsed >= craftDurationMs)
        {
            FinalizeCraft();
        }
        else
        {
            long remainMs = craftDurationMs - elapsed;
            StartCoroutine(CraftCoroutine(remainMs));
        }
    }

    IEnumerator CraftCoroutine(long remainMs)
    {
        var essence = dataBase?.ResolveEssence(currentEssenceID);
        if (essence == null || essence.essenceStage == null) yield break;

        float totalTime = craftDurationSec;
        float elapsed = 0f;
        int stageIndex = 0;

        var sr = spawnedEssence?.GetComponent<SpriteRenderer>();

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            stageIndex = Mathf.Min(Mathf.FloorToInt((elapsed / totalTime) * essence.essenceStage.progressStage.Count), essence.essenceStage.progressStage.Count - 1);

            if (sr != null)
            {
                sr.sprite = essence.essenceStage.progressStage[stageIndex];
                sr.color = essence.color;
            }
            yield return null;
        }
        FinalizeCraft();
    }

    void FinalizeCraft()
    {
        // 연료 1개, 꽃잎 1개 소비 > 에센스 프리팹 생성
        ConsumeOneFuel();
        ConsumeOnePetal();

        if (spawnedEssence != null) Destroy(spawnedEssence);

        EssenceData essence = dataBase?.ResolveEssence(currentEssenceID);
        if (essence != null)
        {
            if (essence.essenceStage != null && essence.essenceStage.progressStage.Count > 0)
            {
                spawnedEssence = new GameObject("EssenceFinal");
                spawnedEssence.transform.SetParent(essenceTransform, false);
                spawnedEssence.transform.localPosition = Vector3.zero;

                var sr = spawnedEssence.AddComponent<SpriteRenderer>();
                sr.sprite = essence.essenceStage.progressStage[^1];
                sr.color = essence.color;
                sr.sortingOrder = 10;
            }
        }
        isMaking = false;
        TillSaveService.Touch(distillerID, SaveSnapshot(essenceReady: true));
    }

    // ============ 저장/복원 ============

    DistillerSaveData SaveSnapshot(bool essenceReady = false)
    {
        DistillerSaveData distillerSaveData = new DistillerSaveData
        {
            id = distillerID,
            isMaking = isMaking,
            craftStartUtcMs = craftStartUtcMs,
            craftDurationMs = craftDurationSec * 1000,
            essenceid = currentEssenceID,
            essenceReady = essenceReady
        };

        // Fuel 기록
        for (int i = 0; i < fuelSlotParent.Count; i++)
        {
            Transform t = fuelSlotParent[i];
            if (t != null && t.childCount > 0)
            {
                TubeItemDisplay display = t.GetChild(0).GetComponent<TubeItemDisplay>();
                if (display != null && display.myItemData != null && display.myItemData.itemName == "Fuel") distillerSaveData.occupiedFuelSlots.Add(i);
            }
        }

        // Petal 기록
        for (int i = 0; i < petalSlotParent.Count; i++)
        {
            Transform t = petalSlotParent[i];
            if (t != null && t.childCount > 0)
            {
                TubeItemDisplay display = t.GetChild(0).GetComponent<TubeItemDisplay>();
                if (display != null && display.myItemData != null)
                {
                    distillerSaveData.petalSlots.Add(new PetalSlotData { index = i, itemID = display.myItemData.id });
                }
            }
        }

        return distillerSaveData;
    }

    public void RebuildFromSave(DistillerSaveData data)
    {
        // 슬롯 치우기
        ClearAllSlots(fuelSlotParent);
        ClearAllSlots(petalSlotParent);
        if (spawnedEssence != null)
        {
            Destroy(spawnedEssence);
            spawnedEssence = null;
        }

        if (!data.essenceReady)
        {
            // 연료 복원
            foreach (int index in data.occupiedFuelSlots)
            {
                if (index < 0 || index >= fuelSlotParent.Count) continue;
                var fuel = dataBase?.ResolveFuel(fuelID); // fuel의 id
                if (fuel is FuelData fuelMat) FuelSpawnToSlot(fuel, index, fuelSlotParent[index]);
            }

            // 꽃잎 복원
            foreach (PetalSlotData p in data.petalSlots)
            {
                if (p.index < 0 || p.index >= petalSlotParent.Count) continue;
                var petal = dataBase?.ResolveMaterial(p.itemID);
                if (petal != null) PetalSpawnToSlot(petal, petalSlotParent[p.index]);
            }
        }
        else
        {
            bool ConsumeOneFuel = false;
            foreach (int index in data.occupiedFuelSlots)
            {
                if (index < 0 || index >= fuelSlotParent.Count) continue;
                var fuel = dataBase?.ResolveFuel(fuelID); // fuel의 id
                if (fuel is FuelData fuelMat)
                {
                    if (!ConsumeOneFuel)
                    {
                        ConsumeOneFuel = true;
                        continue;
                    }
                    FuelSpawnToSlot(fuel, index, fuelSlotParent[index]);
                }
            }
        }


        // 제작 진행/씬 밖 보정
        isMaking = data.isMaking;
        craftStartUtcMs = data.craftStartUtcMs;
        currentEssenceID = data.essenceid;

        if (data.essenceReady && data.essenceid != 0) //  && data.essenceid.HasValue
        {
            var essence = dataBase?.ResolveEssence(data.essenceid);


            if (essence.essenceStage != null && essence.essenceStage.progressStage.Count > 0)
            {
                spawnedEssence = new GameObject("EssenceFinal");
                spawnedEssence.transform.SetParent(essenceTransform, false);
                spawnedEssence.transform.localPosition = Vector3.zero;

                var sr = spawnedEssence.AddComponent<SpriteRenderer>();
                sr.sprite = essence.essenceStage.progressStage[^1];
                sr.color = essence.color;
                sr.sortingOrder = 10;
            }
            isMaking = false;
        }
    }

    // ============= 하위 함수 ===========

    Transform FindFirstEmpty(List<Transform> parents)
    {
        foreach (Transform p in parents)
        {
            if (p != null && p.childCount == 0) return p;
        }
        return null;
    }

    void FuelSpawnToSlot(FuelData data, int slotIndex, Transform slot)
    {
        if (data == null || slot == null) return;
        if (slotIndex < 0 || slotIndex >= data.itemPrefabs.Count) return;

        GameObject prefab = data.itemPrefabs[slotIndex];
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, slot);
        go.transform.localPosition = Vector3.zero;

        TubeItemDisplay display = go.GetComponent<TubeItemDisplay>();
        if (display != null) display.myItemData = data;
    }

    void PetalSpawnToSlot(MaterialData data, Transform slot)
    {
        if (data == null || data.itemPrefab == null || slot == null) return;
        GameObject go = Instantiate(data.itemPrefab, slot);
        go.transform.localPosition = Vector3.zero;
        TubeItemDisplay display = go.GetComponent<TubeItemDisplay>();
        if (display != null) display.myItemData = data;
    }

    void ClearAllSlots(List<Transform> parents)
    {
        foreach (Transform p in parents)
        {
            if (p == null) continue;
            for (int i = p.childCount - 1; i >= 0; i--)
            {
                Destroy(p.GetChild(i).gameObject);
            }
        }
    }
    bool HasAtLeastOneFuel()
    {
        foreach (Transform t in fuelSlotParent)
        {
            if (t != null && t.childCount > 0)
            {
                TubeItemDisplay display = t.GetChild(0).GetComponent<TubeItemDisplay>();
                if (display != null && display.myItemData != null && display.myItemData.itemName == "Fuel") return true;
            }
        }
        return false;
    }

    MaterialData FindFirstPetalMaterial()
    {
        foreach (Transform t in petalSlotParent)
        {
            if (t != null && t.childCount > 0)
            {
                TubeItemDisplay display = t.GetChild(0).GetComponent<TubeItemDisplay>();
                if (display != null && display.myItemData is MaterialData material)
                {
                    return material;
                }
            }
        }

        return null;
    }

    void ConsumeOneFuel()
    {
        for (int i = 0; i < fuelSlotParent.Count; i++)
        {
            Transform t = fuelSlotParent[i];
            if (t != null && t.childCount > 0)
            {
                TubeItemDisplay display = t.GetChild(0).GetComponent<TubeItemDisplay>();
                if (display != null && display.myItemData != null && display.myItemData.itemName == "Fuel")
                {
                    Destroy(t.GetChild(0).gameObject);
                    break;
                }
            }
        }
    }
    void ConsumeOnePetal()
    {
        for (int i = 0; i < petalSlotParent.Count; i++)
        {
            Transform t = petalSlotParent[i];
            if (t != null && t.childCount > 0)
            {
                Destroy(t.GetChild(0).gameObject);
                break;
            }
        }
    }

    bool TryConsumeInventoryFuel(out FuelData fuelData)
    {
        fuelData = null;
        if (InventoryManager.Instance == null) return false;

        int index = InventoryManager.Instance.SelectedSlotIndex;
        if (index < 0 || index >= InventoryManager.Instance.itemSlots.Count) return false;

        ItemSlot slot = InventoryManager.Instance.itemSlots[index];
        if (slot == null || slot.itemData == null) return false;
        if (slot.itemData.itemName != "Fuel") return false;

        fuelData = slot.itemData as FuelData;
        if (fuelData == null) return false;

        return InventoryManager.Instance.RemoveItem(fuelData, 1);
    }
}
