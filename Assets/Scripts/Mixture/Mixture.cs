using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Mixture : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] public ItemDataBase itemDataBase;
    [Header("Slots")]
    [SerializeField] public GameObject baseL;
    [SerializeField] public GameObject baseLChild;
    [SerializeField] public GameObject middleL;
    [SerializeField] public GameObject middleLChild;
    [SerializeField] public GameObject topL;
    [SerializeField] public GameObject topLChild;
    [SerializeField] public List<GameObject> PerfumeL; // 0=Base, 1=middle, 2=top, 3=complete
    [SerializeField] public GameObject punnel;
    [SerializeField] public GameObject punnelChild;
    [SerializeField] public GameObject flowZone;

    [Header("Perfume Itme")]
    [SerializeField] public List<PerfumeData> perfumeDatas;

    [System.NonSerialized] public EssenceData baseData = null;
    [System.NonSerialized] public EssenceData middleData = null;
    [System.NonSerialized] public EssenceData topData = null;
    [System.NonSerialized] public EssenceData pBaseData = null;
    [System.NonSerialized] public EssenceData pMiddleData = null;
    [System.NonSerialized] public EssenceData pTopData = null;
    [System.NonSerialized] public PerfumeData perfumeData = null;
    [Header("Animation")]
    [SerializeField] public Animator baseWaterDropAni;
    [SerializeField] public Animator middleWaterDropAni;
    [SerializeField] public Animator topWaterDropAni;
    [SerializeField] public Animator punnelAni;
    [SerializeField] public Animator pBaseLAni;
    [SerializeField] public Animator pMiddleLAni;
    [SerializeField] public Animator pTopLAni;
    [SerializeField] public Animator perfumeCompleteAni;

    float perfumeWarm;
    float perfumeCool;
    float perfumeRelax;
    [NonSerialized] public bool perfumeIsComplete = false;

    void Start()
    {
        GameSave save = SaveManager.Instance.CurrentSave ?? SaveManager.Load();
        if (save.mixture != null)
            ApplySnapShot(save.mixture);
        else
            punnel.GetComponent<SpriteRenderer>().enabled = true;
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
            NoticeUIManager.Instance.ShowNoticeCanvas("need essence item");
            return;
        }

        if (target.GetComponent<SpriteRenderer>().enabled)
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("Already Exist essence");
            return;
        }

        InventoryManager.Instance.RemoveItem(essenceData, 1);

        EssenceSpawnToSlot(essenceData, target);
        SaveNow();
    }

    public bool PutEssenceInPerfume(EssenceData essenceData, GameObject target, GameObject from)
    {
        if (essenceData == null || target == null) return false;

        var sr = target.GetComponent<SpriteRenderer>();
        //if (sr.enabled == true) return false;
        var srF = from.GetComponent<SpriteRenderer>();
        if (srF.enabled == false) return false;
        //var srC = target.GetComponentInChildren<SpriteRenderer>();

        // sr.enabled = true;
        sr.color = essenceData.color;
        sr.sortingOrder = 1;
        //srC.color = essenceData.color;

        //srF.enabled = false;


        return true;
    }

    public void MakingPerfume(EssenceData baseEssence, EssenceData middleEssence, EssenceData topEssence)
    {
        PerfumeL[0].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[1].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[2].GetComponent<SpriteRenderer>().enabled = false;

        CalculateCapacityAndColor();
        PerfumeL[3].GetComponent<SpriteRenderer>().enabled = true;
        PerfumeL[3].GetComponent<SpriteRenderer>().color = perfumeData.color;
        perfumeIsComplete = true;
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
        // var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        // if (PCompleteL.enabled == true) return true;
        if (perfumeIsComplete) return true;
        return false;
    }

    // ========== 저장 관련 ==============
    public MixtureSaveData CreatSnapShot()
    {
        MixtureSaveData data = new MixtureSaveData();
        data.baseID = (baseData != null ? baseData.id : -1);
        data.middleID = (middleData != null ? middleData.id : -1);
        data.topID = (topData != null ? topData.id : -1);

        data.pBaseID = (pBaseData != null ? pBaseData.id : -1);
        data.pMiddleID = (pMiddleData != null ? pMiddleData.id : -1);
        data.pTopID = (pTopData != null ? pTopData.id : -1);

        data.perfumeComplete = perfumeIsComplete;
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
        GameSave save = SaveManager.Instance.CurrentSave;
        MixtureSaveManager.SaveMixture(save, snap);
        SaveManager.Save(save);
    }

    public void ApplySnapShot(MixtureSaveData data)
    {
        if (data == null)
        {
            return;
        }

        baseData = (data.baseID >= 0) ? itemDataBase.ResolveEssence(data.baseID) : null;
        middleData = (data.middleID >= 0) ? itemDataBase.ResolveEssence(data.middleID) : null;
        topData = (data.topID >= 0) ? itemDataBase.ResolveEssence(data.topID) : null;

        pBaseData = (data.pBaseID >= 0) ? itemDataBase.ResolveEssence(data.pBaseID) : null;
        pMiddleData = (data.pMiddleID >= 0) ? itemDataBase.ResolveEssence(data.pMiddleID) : null;
        pTopData = (data.pTopID >= 0) ? itemDataBase.ResolveEssence(data.pTopID) : null;

        SetSR(baseL, data.baseOn, baseData);
        SetSR(middleL, data.middleOn, middleData);
        SetSR(topL, data.topOn, topData);

        perfumeIsComplete = data.perfumeComplete;
        var p3 = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (perfumeIsComplete) p3.enabled = perfumeIsComplete;
        else
        {
            SetSR(PerfumeL[0], data.pBaseOn, pBaseData);
            SetSR(PerfumeL[1], data.pMiddleOn, pMiddleData);
            SetSR(PerfumeL[2], data.pTopOn, pTopData);
        }
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
        sr.sortingOrder = 1;
    }

    public void CalculateCapacityAndColor()
    {
        if (pBaseData == null || pMiddleData == null || pTopData == null)
            return;

        perfumeWarm = (pBaseData.essenceWarm + pMiddleData.essenceWarm + pTopData.essenceWarm) / 3;
        perfumeCool = (pBaseData.essenceCool + pMiddleData.essenceCool + pTopData.essenceCool) / 3;
        perfumeRelax = (pBaseData.essenceRelax + pMiddleData.essenceRelax + pTopData.essenceRelax) / 3;

        if (perfumeRelax > perfumeWarm && perfumeRelax > perfumeCool) perfumeData = perfumeDatas[0];
        else if (perfumeWarm > perfumeRelax && perfumeWarm > perfumeCool) perfumeData = perfumeDatas[1];
        else if (perfumeCool > perfumeWarm && perfumeCool > perfumeRelax) perfumeData = perfumeDatas[2];
        else if (perfumeRelax == perfumeWarm && perfumeRelax != perfumeCool) perfumeData = perfumeDatas[3];
        else if (perfumeRelax == perfumeCool && perfumeRelax != perfumeWarm) perfumeData = perfumeDatas[4];
        else if (perfumeWarm == perfumeCool && perfumeWarm != perfumeRelax) perfumeData = perfumeDatas[5];
        else perfumeData = perfumeDatas[6];

        perfumeData = ScriptableObject.Instantiate(perfumeData);

        perfumeData.color.r = (pBaseData.color.r + pMiddleData.color.r + pTopData.color.r) / 3;
        perfumeData.color.g = (pBaseData.color.g + pMiddleData.color.g + pTopData.color.g) / 3;
        perfumeData.color.b = (pBaseData.color.b + pMiddleData.color.b + pTopData.color.b) / 3;
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
            sr.sortingOrder = 1;
        }
    }

    public bool PrepareForShaking()
    {
        if (!PerfumeL[0].GetComponent<SpriteRenderer>().enabled ||
           !PerfumeL[1].GetComponent<SpriteRenderer>().enabled ||
           !PerfumeL[2].GetComponent<SpriteRenderer>().enabled)
        {
            return false;
        }

        if (PerfumeL[3].GetComponent<SpriteRenderer>().enabled)
        {
            return true;
        }

        PerfumeL[0].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[1].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[2].GetComponent<SpriteRenderer>().enabled = false;

        PerfumeL[3].GetComponent<SpriteRenderer>().enabled = true;

        CalculateCapacityAndColor();
        if (perfumeData != null)
        {
            PerfumeL[3].GetComponent<SpriteRenderer>().color = perfumeData.color;
        }
        SaveNow();
        return true;
    }
}
