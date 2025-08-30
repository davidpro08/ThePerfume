using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem.Interactions;
using TMPro;

public class Distiller : MonoBehaviour
{
    [Header("Identity/DB")]
    [SerializeField] string distillerID;
    [SerializeField] ItemDataBase dataBase;

    [Header("Slots")]
    [SerializeField] List<GameObject> fuelSlotParent;
    [SerializeField] List<Transform> petalSlotParent;
    [SerializeField] Transform essenceTransform;

    [Header("Sprite")]
    [SerializeField] GameObject Water;

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
        GameSave save = SaveManager.Load();
        DistillerSaveData data = DistillerSaveManager.GetOrCreate(save, distillerID);
        RebuildFromSave(data);

        if (isMaking) StartCatchupOrFinish();
    }

    // ================ 클릭 관련 ================

    public void PlaceFuel()
    {
        if (fuelCount >= fuelSlotParent.Count)
        {
            TillUIManager.Instance.ShowWarningCanvas("No empty fuel slot");
            return;
        }

        if (!TryConsumeInventoryFuel(out ItemData fuelData))
        {
            TillUIManager.Instance.ShowWarningCanvas("need fuel item");
            return;
        }

        if (fuelData.itemType != ItemType.Material && fuelData.id != fuelID)
        {
            TillUIManager.Instance.ShowWarningCanvas("this is not fuel");
            return;
        }

        FuelSpawnToSlot(fuelData);

        GameSave save = SaveManager.Load();
        DistillerSaveManager.Touch(save, distillerID, SaveSnapshot());
        SaveManager.Save(save);

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

        GameSave save = SaveManager.Load();
        DistillerSaveManager.Touch(save, distillerID, SaveSnapshot());
        SaveManager.Save(save);

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

        GameSave save = SaveManager.Load();
        DistillerSaveManager.Touch(save, distillerID, SaveSnapshot());
        SaveManager.Save(save);
    }

    // =========== 제작 로직 ==========

    void TryStartCraft()
    {
        if (isMaking) return;

        bool hasFuel = HasAtLeastOneFuel();
        MaterialData petal = FindFirstPetalMaterial();

        if (!hasFuel || petal == null || petal.essenceData == null) return;

        isMaking = true;
        craftStartUtcMs = DistillerSaveManager.NowUnixMs();
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

        GameSave save = SaveManager.Load();
        DistillerSaveManager.Touch(save, distillerID, SaveSnapshot());
        SaveManager.Save(save);

        StartCatchupOrFinish();
    }

    void StartCatchupOrFinish()
    {
        int craftDurationMs = craftDurationSec * 1000;
        long elapsed = DistillerSaveManager.NowUnixMs() - craftStartUtcMs;
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

        if (spawnedEssence != null)
        {
            Destroy(spawnedEssence);
            Water.GetComponent<SpriteRenderer>().enabled = false;
        }

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

        GameSave save = SaveManager.Load();
        DistillerSaveManager.Touch(save, distillerID, SaveSnapshot(essenceReady: true));
        SaveManager.Save(save);
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
            var slot = fuelSlotParent[i];
            if (slot == null) continue;

            var display = slot.GetComponent<TubeItemDisplay>();
            if (display != null && display.myItemData != null && display.myItemData.itemType == ItemType.Material && display.myItemData.id == fuelID)
            {
                distillerSaveData.occupiedFuelSlots.Add(i);
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
        ClearAllFuel();
        ClearAllSlots(petalSlotParent);
        if (spawnedEssence != null)
        {
            Destroy(spawnedEssence);
            spawnedEssence = null;
        }

        if (!data.essenceReady)
        {
            // 연료 복원
            for (int i = 0; i < data.occupiedFuelSlots.Count; i++)
            {
                int slotIndex = data.occupiedFuelSlots[i];
                if (slotIndex < 0 || slotIndex >= fuelSlotParent.Count) continue;

                var fuelData = dataBase?.ResolveFuel(fuelID);
                FuelSpawnToSlotAtIndex(fuelData, slotIndex);
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
                    FuelSpawnToSlot();
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

    int fuelCount = 0;
    void FuelSpawnToSlot(ItemData fuelData = null)
    {
        if (fuelCount < fuelSlotParent.Count)
        {
            GameObject slotGO = fuelSlotParent[fuelCount];
            TubeItemDisplay display = slotGO.GetComponent<TubeItemDisplay>();
            if (display == null) display = slotGO.AddComponent<TubeItemDisplay>();

            display.myItemData = fuelData;

            var sr = slotGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;

            fuelCount++;
        }
    }

    void PetalSpawnToSlot(MaterialData data, Transform slot)
    {
        if (data == null || data.itemPrefab == null || slot == null) return;
        GameObject go = Instantiate(data.itemPrefab, slot);
        go.transform.localPosition = Vector3.zero;
        TubeItemDisplay display = go.GetComponent<TubeItemDisplay>();
        if (display != null) display.myItemData = data;
        Water.GetComponent<SpriteRenderer>().enabled = true;
    }

    void ClearAllSlots(List<Transform> parents)
    {
        foreach (Transform p in parents)
        {
            if (p == null) continue;

            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    void ClearAllFuel()
    {
        foreach (var slot in fuelSlotParent)
        {
            var sr = slot.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
        fuelCount = 0;
    }

    bool HasAtLeastOneFuel()
    {
        foreach (GameObject t in fuelSlotParent)
        {
            if (t != null)
            {
                TubeItemDisplay display = t.GetComponent<TubeItemDisplay>();
                if (display != null && display.myItemData != null && display.myItemData.id == fuelID) return true;
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
        if (fuelCount > 0)
        {
            fuelCount--;
            var sr = fuelSlotParent[fuelCount].GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
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

    bool TryConsumeInventoryFuel(out ItemData fuelData)
    {
        fuelData = null;
        if (InventoryManager.Instance == null) return false;

        int index = InventoryManager.Instance.SelectedSlotIndex;
        if (index < 0 || index >= InventoryManager.Instance.itemSlots.Count) return false;

        ItemSlot slot = InventoryManager.Instance.itemSlots[index];
        if (slot == null || slot.itemData == null) return false;

        if (slot.itemData.id != fuelID && slot.itemData.itemType != ItemType.Material) return false;

        fuelData = slot.itemData;

        return InventoryManager.Instance.RemoveItem(fuelData, 1);
    }

    void FuelSpawnToSlotAtIndex(ItemData fuelData, int index)
    {
        if (index < 0 || index >= fuelSlotParent.Count) return;
        GameObject slotGO = fuelSlotParent[index];
        TubeItemDisplay display = slotGO.GetComponent<TubeItemDisplay>();
        if (display == null) display = slotGO.AddComponent<TubeItemDisplay>();

        display.myItemData = fuelData;

        var sr = slotGO.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }
}
