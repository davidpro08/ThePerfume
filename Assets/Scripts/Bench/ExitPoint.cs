using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ExitPoint : MonoBehaviour
{
    [Header("나가기 설정")]
    [SerializeField] private string targetSceneName = "lab";
    [SerializeField] private LayerMask clickableLayer;

    private Collider2D _collider2D;

    void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        if (_collider2D == null) enabled = false;
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, clickableLayer);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (FlowerManager.Instance == null) Debug.Log("[HandleExit] TillUIManager.Instance == null");

                if (!InventoryUIManager.isFullInventoryOpen && !FlowerManager.Instance.blockingCanvasOpen && !BenchUIManager.Instance.warningCanvasOpen && FlowerManager.Instance.IsExitable() && !BenchUIManager.Instance.HasSpawnedItemOnTray())
                {
                    if (string.IsNullOrEmpty(targetSceneName))
                    {
                        return;
                    }
                    SceneChanger.Instance.MoveToScene();
                }
                else
                {
                    if (!FlowerManager.Instance.IsExitable())
                    {
                        BenchUIManager.Instance.ShowWarningCanvas("bowl에 꽃잎이 남아있습니다.");
                        Debug.LogWarning($"IsExitable : Bowl에 꽃잎이 남아있음");
                    }
                    if (BenchUIManager.Instance.HasSpawnedItemOnTray())
                    {
                        BenchUIManager.Instance.ShowWarningCanvas("Tray 위에 작물이 남아있습니다.");
                        Debug.LogWarning($"HasSpawnedItemOnTray : Tray 위에 작물이 있음");
                    }
                }
            }
        }
    }

    public void HandleExit()
    {
        if (FlowerManager.Instance == null) Debug.Log("[HandleExit] TillUIManager.Instance == null");

        if (!InventoryUIManager.isFullInventoryOpen && !FlowerManager.Instance.blockingCanvasOpen && !BenchUIManager.Instance.warningCanvasOpen && FlowerManager.Instance.IsExitable() && !BenchUIManager.Instance.HasSpawnedItemOnTray())
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                return;
            }
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            if (!FlowerManager.Instance.IsExitable())
            {
                BenchUIManager.Instance.ShowWarningCanvas("bowl에 꽃잎이 남아있습니다.");
                Debug.LogWarning($"IsExitable : Bowl에 꽃잎이 남아있음");
            }
            if (BenchUIManager.Instance.HasSpawnedItemOnTray())
            {
                BenchUIManager.Instance.ShowWarningCanvas("Tray 위에 작물이 남아있습니다.");
                Debug.LogWarning($"HasSpawnedItemOnTray : Tray 위에 작물이 있음");
            }
        }
    }
}
