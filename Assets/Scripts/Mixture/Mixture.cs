using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mixture : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] public ItemDataBase itemDataBase;
    [Header("Slots")]
    [SerializeField] public GameObject baseL;
    [SerializeField] public GameObject middleL;
    [SerializeField] public GameObject topL;
    [SerializeField] public List<GameObject> PerfumeL; // 0=Base, 1=middle, 2=top, 3=complete
    [SerializeField] public GameObject punnel;
    [SerializeField] public GameObject flowZone;

    [Header("Perfume Itme")]
    [SerializeField] public List<PerfumeData> perfumeDatas;

    public EssenceData baseData = null;
    public EssenceData middleData = null;
    public EssenceData topData = null;
    public PerfumeData perfumeData = null;

    float perfumeWarm;
    float perfumeCool;
    float perfumeRelax;

    void Start()
    {
        GameSave save = SaveManager.Load();
        ApplySnapShot(save.mixture);
    }

    // =========== 클릭 관련 / 생성 관련 =============
    public void PlaceEssence(EssenceData essenceData, GameObject target)
    {
        if (essenceData == null)
        {
            Debug.LogError("EssenceData == null");
            return;
        }

        ItemSlot slotToRemove = InventoryManager.Instance.itemSlots.Find(slot => slot.itemData != null && slot.itemData.id == essenceData.id);

        if (slotToRemove == null)
        {
            TillUIManager.Instance.ShowWarningCanvas("need essence item");
            return;
        }

        if (target.GetComponent<SpriteRenderer>().enabled)
        {
            TillUIManager.Instance.ShowWarningCanvas("Already Exist essence");
            return;
        }

        InventoryManager.Instance.RemoveItem(essenceData, 1);

        EssenceSpawnToSlot(essenceData, target);
        SaveNow();
    }

    public void PutEssenceInPerfume(EssenceData essenceData, GameObject target, GameObject from)
    {
        if (essenceData == null || target == null) return;

        var sr = target.GetComponent<SpriteRenderer>();
        if (sr.enabled == true) return;
        var srF = from.GetComponent<SpriteRenderer>();
        if (srF.enabled == false) return;

        sr.enabled = true;
        sr.color = essenceData.color;
        sr.sortingOrder = 10;

        srF.enabled = false;
        SaveNow();
    }

    public void MakingPerfume(EssenceData baseEssence, EssenceData middleEssence, EssenceData topEssence)
    {
        PerfumeL[0].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[1].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[2].GetComponent<SpriteRenderer>().enabled = false;

        CalculateCapacityAndColor();
        PerfumeL[3].GetComponent<SpriteRenderer>().enabled = true;
        PerfumeL[3].GetComponent<SpriteRenderer>().color = perfumeData.color;

        SaveNow();
    }

    // =========== 판단 로직 =============
    public bool CanBePBaseL()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.enabled == false && PCompleteL.enabled == false) return true;
        return false;
    }

    public bool CanBePMiddleL()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PMiddleL = PerfumeL[1].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.enabled == true && PMiddleL.enabled == false && PCompleteL.enabled == false) return true;

        return false;
    }

    public bool CanBePTopL()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PMiddleL = PerfumeL[1].GetComponent<SpriteRenderer>();
        var PTopL = PerfumeL[2].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.enabled == true && PMiddleL.enabled == true && PTopL.enabled == false && PCompleteL.enabled == false) return true;

        return false;
    }

    public bool CanRemovePunnel()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PMiddleL = PerfumeL[1].GetComponent<SpriteRenderer>();
        var PTopL = PerfumeL[2].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.enabled == true && PMiddleL.enabled == true && PTopL.enabled == true && PCompleteL.enabled == false && punnel.GetComponent<SpriteRenderer>().enabled == true) return true;
        return false;
    }

    public bool CanMakePerfume()
    {
        if (punnel.GetComponent<SpriteRenderer>().enabled == false) return true;
        return false;
    }

    public bool CanGainPerfume()
    {
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PCompleteL.enabled == true) return true;
        return false;
    }

    // ========== 저장 관련 ==============
    public MixtureSaveData CreatSnapShot()
    {
        MixtureSaveData data = new MixtureSaveData();
        data.baseEssenceID = (baseData != null ? baseData.id : -1);
        data.middleEssenceID = (middleData != null ? middleData.id : -1);
        data.topEssenceID = (topData != null ? topData.id : -1);

        data.perfumeComplete = PerfumeL[3].GetComponent<SpriteRenderer>().enabled;
        data.perfumeID = perfumeData != null ? perfumeData.id : -1;

        if (perfumeData != null)
        {
            data.colorR = perfumeData.color.r;
            data.colorG = perfumeData.color.g;
            data.colorB = perfumeData.color.b;

            data.warm = perfumeData.perfumeWarm;
            data.cool = perfumeData.perfumeCool;
            data.relax = perfumeData.perfumeRelax;
        }

        data.baseOn = baseL != null && baseL.GetComponent<SpriteRenderer>().enabled;
        data.middleOn = middleL != null && middleL.GetComponent<SpriteRenderer>().enabled;
        data.topOn = topL != null && topL.GetComponent<SpriteRenderer>().enabled;

        data.pBaseOn = PerfumeL[0].GetComponent<SpriteRenderer>().enabled;
        data.pMiddleOn = PerfumeL[1].GetComponent<SpriteRenderer>().enabled;
        data.pTopOn = PerfumeL[2].GetComponent<SpriteRenderer>().enabled;
        data.punnelOn = punnel != null && punnel.GetComponent<SpriteRenderer>().enabled;

        return data;
    }

    public void SaveNow()
    {
        var snap = CreatSnapShot();
        GameSave save = SaveManager.Load();
        MixtureSaveManager.SaveMixture(save, snap);
        SaveManager.Save(save);
    }

    public void ApplySnapShot(MixtureSaveData data)
    {
        if (data == null) return;

        baseData = (data.baseEssenceID >= 0) ? itemDataBase.ResolveEssence(data.baseEssenceID) : null;
        middleData = (data.middleEssenceID >= 0) ? itemDataBase.ResolveEssence(data.middleEssenceID) : null;
        topData = (data.topEssenceID >= 0) ? itemDataBase.ResolveEssence(data.topEssenceID) : null;

        SetSR(baseL, data.baseOn, baseData);
        SetSR(middleL, data.middleOn, middleData);
        SetSR(topL, data.topOn, topData);

        SetSR(PerfumeL[0], data.pBaseOn, baseData);
        SetSR(PerfumeL[1], data.pMiddleOn, middleData);
        SetSR(PerfumeL[2], data.pTopOn, topData);

        var p3 = PerfumeL[3].GetComponent<SpriteRenderer>();
        p3.enabled = data.perfumeComplete;

        if (data.perfumeID >= 0)
        {
            perfumeData = perfumeDatas.Find(p => p.id == data.perfumeID);
            if (perfumeData != null)
            {
                perfumeData = ScriptableObject.Instantiate(perfumeData);
                perfumeData.color = new Color(data.colorR, data.colorG, data.colorB, 1);
                perfumeData.perfumeWarm = data.warm;
                perfumeData.perfumeCool = data.cool;
                perfumeData.perfumeRelax = data.relax;

                PerfumeL[3].GetComponent<SpriteRenderer>().color = perfumeData.color;
            }
        }

        if (punnel != null)
        {
            var psr = punnel.GetComponent<SpriteRenderer>();
            if (psr != null) psr.enabled = data.punnelOn;
        }
    }

    // =========== 보조 함수 =============
    void EssenceSpawnToSlot(EssenceData data, GameObject target)
    {
        if (data == null || target == null) return;

        var sr = target.GetComponent<SpriteRenderer>();
        if (sr.enabled || sr == null) return;
        sr.enabled = true;
        sr.color = data.color;
        sr.sortingOrder = 10;
    }

    public void CalculateCapacityAndColor()
    {
        if (baseData == null || middleData == null || topData == null)
            return;

        perfumeWarm = (baseData.essenceWarm + middleData.essenceWarm + topData.essenceWarm) / 3;
        perfumeCool = (baseData.essenceCool + middleData.essenceCool + topData.essenceCool) / 3;
        perfumeRelax = (baseData.essenceRelax + middleData.essenceRelax + topData.essenceRelax) / 3;

        if (perfumeRelax > perfumeWarm && perfumeRelax > perfumeCool) perfumeData = perfumeDatas[0];
        else if (perfumeWarm > perfumeRelax && perfumeWarm > perfumeCool) perfumeData = perfumeDatas[1];
        else if (perfumeCool > perfumeWarm && perfumeCool > perfumeRelax) perfumeData = perfumeDatas[2];
        else if (perfumeRelax == perfumeWarm && perfumeRelax != perfumeCool) perfumeData = perfumeDatas[3];
        else if (perfumeRelax == perfumeCool && perfumeRelax != perfumeWarm) perfumeData = perfumeDatas[4];
        else if (perfumeWarm == perfumeCool && perfumeWarm != perfumeRelax) perfumeData = perfumeDatas[5];
        else perfumeData = perfumeDatas[6];

        perfumeData = ScriptableObject.Instantiate(perfumeData);

        perfumeData.color.r = (baseData.color.r + middleData.color.r + topData.color.r) / 3;
        perfumeData.color.g = (baseData.color.g + middleData.color.g + topData.color.g) / 3;
        perfumeData.color.b = (baseData.color.b + middleData.color.b + topData.color.b) / 3;
        perfumeData.color.a = 1f;

        perfumeData.perfumeRelax = perfumeRelax;
        perfumeData.perfumeWarm = perfumeWarm;
        perfumeData.perfumeCool = perfumeCool;
    }

    void SetSR(GameObject slot, bool on, EssenceData essenceData)
    {
        if (slot == null) return;
        var sr = slot.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.enabled = on;
        if (on && essenceData != null)
        {
            sr.color = essenceData.color;
            sr.sortingOrder = 10;
        }
    }
}
