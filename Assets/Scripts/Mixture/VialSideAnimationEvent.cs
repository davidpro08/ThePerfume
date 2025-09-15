using UnityEngine;

public class VialSideAnimationEvent : MonoBehaviour
{
    public float delay = 0.9f;
    public GameObject vialSide;

    private void OnEnable()
    {
        Invoke(nameof(HideParent), delay);
    }

    void HideParent()
    {
        if(vialSide != null) vialSide.SetActive(false);
        else transform.parent.gameObject.SetActive(false);
    }





    //public Mixture mixture;

    // 어떤 이유이선지 애니메이션 이벤트가 발동 안됨..

    //public void OnFillAnimationEnd()
    //{
    //    Debug.Log("Fill Animation Event Called!");
    //    if (mixture != null) mixture.OnFillAnimationEnd();
    //}
}
