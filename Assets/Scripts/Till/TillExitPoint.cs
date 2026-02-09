using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TillExitPoint : MonoBehaviour
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
        if (!InventoryUIManager.isFullInventoryOpen && ClickTargetAssence.isPouring == false)
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                return;
            }

            InventorySaveManager.SaveInventory(SaveManager.Instance.CurrentSave, InventoryManager.Instance, this, immediate: true);

            // 로딩 UI를 사용하여 씬 전환
            if (LoadingUIManager.Instance != null)
            {
                LoadingUIManager.Instance.LoadScene(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
        if (ClickTargetAssence.isPouring)
        {
            NoticeUIManager.Instance.ShowNoticeCanvas("향수를 붓는 중에는 나갈 수 없습니다.");
        }
    }
}
