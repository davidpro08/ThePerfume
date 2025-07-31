using UnityEngine;

public class FlowerPetal : MonoBehaviour
{
    public bool isRemoved = false; // 이미 뜯겼냐?

    // 꽃잎 클릭 + 드래그
    private void OnMouseDown()
    {
        if (isRemoved) return;

        // 꽃잎 뜯기
        //

        isRemoved = true;
        gameObject.SetActive(false);

        // bowl에 꽃잎 넣는 애니메이션 추가
        //

        // bowl 아이템 개수 증가
        //
    }
}
