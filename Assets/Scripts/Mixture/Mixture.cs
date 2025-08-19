using UnityEngine;

public class Mixture : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] Transform baseTransform;
    [SerializeField] Transform middleTransform;
    [SerializeField] Transform topTransform;
    [SerializeField] Transform essenceTransform;
    [SerializeField] Transform funnelTransform;

    // =========== 클릭 관련 =============
    public void PlaceEssence(EssenceData essenceData)
    {
        if (essenceData == null || essenceData.color == null)
        {
            //
        }
    }
}
