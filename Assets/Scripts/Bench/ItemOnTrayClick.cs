using UnityEngine;
using UnityEngine.InputSystem;

public class ItemOnTrayClick : MonoBehaviour
{
    public ItemData myItemData;

    void OnMouseDown()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("UI 위 클릭 무시");
            return;
        }

        Debug.Log($"{gameObject.name} 클릭");

        if (FlowerManager.Instance != null)
        {
            FlowerManager.Instance.StartHandling((myItemData));
        }
        else
        {
            Debug.Log($"FLowerManager.Instance == null");
        }
    }
}
