using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;

public class BugDetect : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckWhatIsClicked();
        }
    }

    void CheckWhatIsClicked()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            Debug.Log($"======== 클릭 감지 : 총 {results.Count}개 =======");
            foreach (var result in results)
            {
                Debug.Log($"감지된 오브젝트: {result.gameObject.name} (레이어:{LayerMask.LayerToName(result.gameObject.layer)})");

                if (result.module is UnityEngine.UI.GraphicRaycaster)
                {
                    Debug.Log($"**UI가 앞을 막고 있음: {result.gameObject.name}**");
                }
            }
        }
        else
        {
            Debug.Log("감지된것이 없음");
        }
    }
}
