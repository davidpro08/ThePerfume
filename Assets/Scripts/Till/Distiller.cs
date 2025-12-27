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
    private List<bool> occupiedFuelSlots = new List<bool> { false, false, false };
    [SerializeField] List<Transform> petalSlotParent;
    [SerializeField] GameObject essenceGO;
    private SpriteRenderer essenceSR;

    [Header("Sprite")]
    [SerializeField] GameObject Water;

    [Header("Animation")]
    [SerializeField] private GameObject waterDrop;
    [SerializeField] private SpriteRenderer tubeFillRenderer;
    [SerializeField] private Animator tubeFillAnimator;

    [Header("Craft")]
    [SerializeField] int craftDurationSec = 1;
    //임시 아이템 아이디 정해지면 정리해야함
    [SerializeField] int fuelID = 99;

    bool isMaking;
    long craftStartUtcMs;
    int currentEssenceID;
    readonly List<GameObject> spawndItems = new List<GameObject>();

    void Start()
    {
        if (string.IsNullOrEmpty(distillerID) && GameContext.Instance != null)
        {
            distillerID = GameContext.Instance.SelectedDistillerID;
        }
        GameSave save = SaveManager.Instance.CurrentSave;
        DistillerSaveData data = DistillerSaveManager.GetOrCreate(save, distillerID);
        RebuildFromSave(data);

        if (isMaking) StartCatchupOrFinish();
    }

    void Awake()
    {
        if (essenceGO != null) essenceSR = essenceGO.GetComponent<SpriteRenderer>();
    }

    // ================ 클릭 관련 ================

    public void PlaceFuel()
    {
        if (fuelCount >= fuelSlotParent.Count)
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("No empty fuel slot");
            return;
        }

        if (!TryConsumeInventoryFuel(out ItemData fuelData))
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("need fuel item");
            return;
        }

        if (fuelData.itemType != ItemType.Material || fuelData.id != fuelID)
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("this is not fuel");
            return;
        }

        FuelSpawnToSlot(fuelCount);
        occupiedFuelSlots[fuelCount] = true;
        fuelCount++;

        GameSave save = SaveManager.Instance.CurrentSave;
        DistillerSaveManager.Touch(save, distillerID, SaveSnapshot());
        SaveManager.Save(save);

        TryStartCraft();
    }

    public void PlacePetal(MaterialData petalData)
    {
        if (essenceGO != null)
        {
            EssenceData essence = dataBase?.ResolveEssence(currentEssenceID);
            if (essence != null)
            {
                if (essenceSR != null && essenceSR.enabled)
                {
                    NoticeUIManager.Instance.ShowNoticeCanvas("Essence is already");
                    return;
                }
            }

        }

        if (petalData == null || !petalData.itemPrefab)
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("need petal item");
            return;
        }

        var slot = FindFirstEmpty(petalSlotParent);
        if (slot == null)
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("no empty petal slot");
            return;
        }
        if (!InventoryManager.Instance.RemoveItem(petalData, 1))
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("need petal item");
            return;
        }

        PetalSpawnToSlot(petalData, slot);

        StartCoroutine(SaveNextFrame());

        TryStartCraft();
    }

    IEnumerator SaveNextFrame()
    {
        yield return null;
        GameSave save = SaveManager.Instance.CurrentSave;
        DistillerSaveManager.Touch(save, distillerID, SaveSnapshot());
        SaveManager.Save(save);
    }

    public void OnEssenceClicked()
    {
        if (isMaking || essenceGO == null || currentEssenceID == 0) return; //  || !currentEssenceID.HasValue

        tubeFillAnimator.Play("EssenceBottle_empty", 0, 0f);
        EssenceData essence = dataBase?.ResolveEssence(currentEssenceID);
        if (essence == null)
        {
            Debug.Log("Essence id invalid");
            return;
        }

        InventoryManager.Instance.AddItem(essence, 1);

        essenceSR.enabled = false;
        currentEssenceID = 0;
        isMaking = false;

        GameSave save = SaveManager.Instance.CurrentSave;
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

        if (essence != null)
        {
            if (essenceSR != null) essenceSR.enabled = false;
            essenceSR.color = essence.color;

            // 색깔 지정
            tubeFillRenderer.color = essence.color;
            var DropRenderer = waterDrop.GetComponent<SpriteRenderer>();
            if (DropRenderer != null) DropRenderer.color = essence.color;

            essenceSR.sortingOrder = 0;
        }

        GameSave save = SaveManager.Instance.CurrentSave;
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
        if (essence == null) yield break;

        float totalTime = craftDurationSec;
        float elapsed = 0f;

        // 물방울 애니메이션 오브젝트 활성화
        waterDrop.SetActive(true);

        // 튜브 차는 애니메이션
        tubeFillAnimator.SetTrigger("StartFill");


        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        waterDrop.SetActive(false);

        FinalizeCraft();
    }

    void FinalizeCraft()
    {
        // 연료 1개, 꽃잎 1개 소비 > 에센스 프리팹 생성
        if (fuelCount <= 0) return;
        ConsumeOneFuel();
        ConsumeOnePetal();

        EssenceData essence = dataBase?.ResolveEssence(currentEssenceID);
        if (essenceSR != null && essence != null)
        {
            essenceSR.color = essence.color;
            essenceSR.sortingOrder = 0;
            essenceSR.enabled = true;

        }

        var waterSR = Water?.GetComponent<SpriteRenderer>();
        if (waterSR != null) waterSR.enabled = false;

        isMaking = false;

        GameSave save = SaveManager.Instance.CurrentSave;
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
            essenceid = (currentEssenceID >= 0) ? currentEssenceID : -1,
            essenceReady = essenceReady,
            // Fuel 기록
            occupiedFuelSlots = new List<bool>(occupiedFuelSlots),
            petalSlots = new List<PetalSlotData>()
        };

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
        ClearAllSlots(petalSlotParent);

        ClearAllFuel();

        if (essenceSR != null) essenceSR.enabled = false;

        fuelCount = 0;

        for (int i = 0; i < data.occupiedFuelSlots.Count; i++)
        {
            occupiedFuelSlots[i] = data.occupiedFuelSlots[i];
            var sr = fuelSlotParent[i].GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = occupiedFuelSlots[i];
            if (occupiedFuelSlots[i]) fuelCount++;
        }

        if (!data.essenceReady)
        {
            // 꽃잎 복원
            foreach (PetalSlotData p in data.petalSlots)
            {
                if (p.index < 0 || p.index >= petalSlotParent.Count) continue;
                var petal = dataBase?.ResolveMaterial(p.itemID);
                if (petal != null) PetalSpawnToSlot(petal, petalSlotParent[p.index]);
            }
        }

        // 제작 진행/씬 밖 보정
        isMaking = data.isMaking;
        craftStartUtcMs = data.craftStartUtcMs;
        currentEssenceID = data.essenceid;

        if (essenceSR != null)
        {
            if (data.essenceReady && data.essenceid >= 0) //  && data.essenceid.HasValue
            {
                var essence = dataBase?.ResolveEssence(data.essenceid);

                if (essence != null)
                {
                    essenceSR.color = essence.color;
                    essenceSR.sortingOrder = 0;
                    essenceSR.enabled = true;

                    var waterSR = Water?.GetComponent<SpriteRenderer>();
                    if (waterSR != null) waterSR.enabled = false;
                }
                isMaking = false;
            }
            else essenceSR.enabled = false;
        }

        bool CanResume = !data.essenceReady && HasAtLeastOneFuel() && (FindFirstPetalMaterial() != null);

        if (CanResume)
        {
            if (!isMaking) isMaking = true;

            if (craftStartUtcMs <= 0) craftStartUtcMs = DistillerSaveManager.NowUnixMs();

            if (currentEssenceID < 0 || currentEssenceID == 0)
            {
                var petal = FindFirstPetalMaterial();
                if (petal != null && petal.essenceData != null)
                    currentEssenceID = petal.essenceData.id;
            }
        }
        else
        {
            isMaking = false;

            var waterSR = Water?.GetComponent<SpriteRenderer>();
            if (waterSR != null) waterSR.enabled = (FindFirstPetalMaterial() != null);
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
    void FuelSpawnToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= fuelSlotParent.Count) return;

        var slotGO = fuelSlotParent[slotIndex];

        var sr = slotGO.GetComponent<SpriteRenderer>();
        if (sr == null) sr = slotGO.AddComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
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

            for (int i = p.childCount - 1; i >= 0; i--)
            {
                Destroy(p.GetChild(i).gameObject);
            }

            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    void ClearAllFuel()
    {
        for (int i = 0; i < fuelSlotParent.Count; i++)
        {
            occupiedFuelSlots[i] = false;
            var sr = fuelSlotParent[i].GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
        fuelCount = 0;
    }

    bool HasAtLeastOneFuel()
    {
        foreach (bool slot in occupiedFuelSlots)
        {
            if (slot) return true;

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

        for (int i = fuelSlotParent.Count - 1; i >= 0; i--)
        {
            if (occupiedFuelSlots[i])
            {
                occupiedFuelSlots[i] = false;
                var sr = fuelSlotParent[i].GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
                break;
            }
        }
        fuelCount = Mathf.Max(0, fuelCount - 1);
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

        if (slot.itemData.id != fuelID || slot.itemData.itemType != ItemType.Material) return false;

        fuelData = slot.itemData;

        return InventoryManager.Instance.RemoveItem(fuelData, 1);
    }
}
