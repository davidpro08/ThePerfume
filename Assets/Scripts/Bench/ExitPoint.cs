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
                HandleExit();
            }
        }
    }

    public void HandleExit()
    {
        if (!FlowerManager.Instance.blockingCanvasOpen && !BenchInventoryUIManager.Instance.warningCanvasOpen && FlowerManager.Instance.IsExitable() && !BenchInventoryUIManager.Instance.HasSpawnedItemOnTray())
        {
            if (BenchInventoryUIManager.Instance != null)
            {
                // ==================================================
                //BenchInventoryUIManager.Instance.CloseAllUI(true);
                // ==================================================
            }

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
                BenchInventoryUIManager.Instance.ShowWarningCanvas("bowl에 꽃잎이 남아있습니다.");
                Debug.LogWarning($"IsExitable : Bowl에 꽃잎이 남아있음");
            }
            if (BenchInventoryUIManager.Instance.HasSpawnedItemOnTray())
            {
                BenchInventoryUIManager.Instance.ShowWarningCanvas("Tray 위에 작물이 남아있습니다.");
                Debug.LogWarning($"HasSpawnedItemOnTray : Tray 위에 작물이 있음");
            }
        }
    }
}
