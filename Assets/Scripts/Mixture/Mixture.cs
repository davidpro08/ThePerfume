using System.Collections.Generic;
using UnityEngine;

public class Mixture : MonoBehaviour
{
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

    // =========== 클릭 관련 / 생성 관련 =============
    public void PlaceEssence(EssenceData essenceData, GameObject target)
    {
        // if (essenceData == null)
        // {
        //     TillUIManager.Instance.ShowWarningCanvas("need Essence item");
        //     return;
        // }

        if (target.GetComponent<SpriteRenderer>().enabled == true)
        {
            TillUIManager.Instance.ShowWarningCanvas("Already essence exist");
            return;
        }

        if (!InventoryManager.Instance.RemoveItem(essenceData, 1))
        {
            TillUIManager.Instance.ShowWarningCanvas("need essence item");
            return;
        }

        EssenceSpawnToSlot(essenceData, target);
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
    }

    public void MakingPerfume(EssenceData baseEssence, EssenceData middleEssence, EssenceData topEssence)
    {
        PerfumeL[0].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[1].GetComponent<SpriteRenderer>().enabled = false;
        PerfumeL[2].GetComponent<SpriteRenderer>().enabled = false;

        CalculateCapacityAndColor();
        PerfumeL[3].GetComponent<SpriteRenderer>().enabled = true;
        PerfumeL[3].GetComponent<SpriteRenderer>().color = perfumeData.color;
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

    // =========== 보조 함수 =============
    void EssenceSpawnToSlot(EssenceData data, GameObject target)
    {
        if (data == null || data.color == null || target == null) return;

        var sr = target.GetComponent<SpriteRenderer>();
        if (sr.enabled == true) return;
        sr.enabled = true;
        sr.color = data.color;
        sr.sortingOrder = 10;
    }

    public void CalculateCapacityAndColor()
    {
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
}
