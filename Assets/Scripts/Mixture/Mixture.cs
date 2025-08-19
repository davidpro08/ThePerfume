using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Mixture : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] public GameObject baseL;
    [SerializeField] public GameObject middleL;
    [SerializeField] public GameObject topL;
    [SerializeField] public List<GameObject> PerfumeL; // 0=Base, 1=middle, 2=top, 3=complete
    [SerializeField] public GameObject funnel;
    [SerializeField] public GameObject flowZone;

    // =========== 클릭 관련 / 생성 관련 =============
    public void PlaceEssence(EssenceData essenceData, GameObject target)
    {
        if (essenceData == null || !essenceData.mixtureTubeSprite)
        {
            TillUIManager.Instance.ShowWarningCanvas("need Essence item");
            return;
        }

        if (target.GetComponent<SpriteRenderer>().sprite != null)
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

    public void PutBaseInPerfume()
    {
        //
    }

    public void PutMiddleInPerfume()
    {
        //
    }

    public void PutTopInPerfume()
    {
        //
    }

    // =========== 판단 로직 =============
    public bool CanBePBaseL()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.sprite == null && PCompleteL == null) return true;
        return false;
    }

    public bool CanBePMiddleL()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PMiddleL = PerfumeL[1].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.sprite != null && PMiddleL == null && PCompleteL == null) return true;
        return false;
    }

    public bool CanBePTopL()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PMiddleL = PerfumeL[1].GetComponent<SpriteRenderer>();
        var PTopL = PerfumeL[2].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.sprite != null && PMiddleL != null && PTopL == null && PCompleteL == null) return true;
        return false;
    }

    public bool CanMakePCompleteL()
    {
        var PBaseL = PerfumeL[0].GetComponent<SpriteRenderer>();
        var PMiddleL = PerfumeL[1].GetComponent<SpriteRenderer>();
        var PTopL = PerfumeL[2].GetComponent<SpriteRenderer>();
        var PCompleteL = PerfumeL[3].GetComponent<SpriteRenderer>();
        if (PBaseL.sprite != null && PMiddleL != null && PTopL != null && PCompleteL == null) return true;
        return false;
    }

    // =========== 보조 함수 =============
    void EssenceSpawnToSlot(EssenceData data, GameObject target)
    {
        if (data == null || data.mixtureTubeSprite == null || target == null) return;

        if (target.GetComponent<Sprite>() != null) return;
        var sr = target.GetComponent<SpriteRenderer>();
        sr.sprite = data.mixtureTubeSprite;
        sr.color = data.color;
        sr.sortingOrder = 10;
    }
}
