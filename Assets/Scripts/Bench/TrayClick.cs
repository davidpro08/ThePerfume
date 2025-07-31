using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TrayClick : MonoBehaviour
{
    [SerializeField] private LayerMask InteractableLayer;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"Tray click detect");
            // UI가 열려있으면 월드 오브젝트 클릭 무시
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log($"UI open");
                return;
            }

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, InteractableLayer);

            if (hit2D.collider != null && hit2D.collider.gameObject == gameObject)
            {
                Debug.Log($"Tray click");
                if (BenchInventoryUIManager.Instance == null)
                {
                    Debug.Log($"Instance null");
                    return;
                }

                if (BenchInventoryUIManager.Instance.HasSpawnedItemOnTray())
                {
                    Debug.Log($"트레이에 아이템 있어서 인벤토리 열 수 없음");
                    return;
                }
                Debug.Log($"Tray OpenInventory call");
                BenchInventoryUIManager.Instance.OpenInventory();
            }
        }
    }
}
