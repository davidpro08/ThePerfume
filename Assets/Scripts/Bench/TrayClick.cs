using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TrayClick : MonoBehaviour
{
    [SerializeField] private LayerMask InteractableLayer;
    private string trayName = "Tray";
    private string bowlName = "Bowl";

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"Tray/bowl click detect");
            // UI가 열려있으면 월드 오브젝트 클릭 무시
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log($"UI open");
                return;
            }

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit2D = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, InteractableLayer);

            if (hit2D.collider != null)
            {
                string clickedObjectName = hit2D.collider.gameObject.name;
                Debug.Log($"{clickedObjectName} clicked");

                if (clickedObjectName == trayName)
                {
                    if (BenchInventoryUIManager.Instance == null)
                    {
                        Debug.Log($"Instance null");
                        return;
                    }
                    if (BenchInventoryUIManager.Instance.HasSpawnedItemOnTray())
                    {
                        Debug.Log("트레이에 아이템 있어서 인벤토리 못열음");
                        return;
                    }

                    Debug.Log($"Tray OpenInventory call");
                    BenchInventoryUIManager.Instance.OpenInventory();
                }
                else if (clickedObjectName == bowlName)
                {
                    if (FlowerManager.Instance == null)
                    {
                        Debug.Log("FlowerManager.Instance == null");
                        return;
                    }
                    FlowerManager.Instance.OnBowlClicked();
                }
            }
        }
    }
}
