using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
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
        if (!InventoryUIManager.isFullInventoryOpen && !TillUIManager.Instance.isWarningCanvasOpen)
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                return;
            }
            SceneChanger.Instance.ResetDistillerID();
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            // 근데 이걸 닫을 때 이걸 확인해야하나..? 닫아도 계속 유지되도록 하는거 아닌가
        }
    }
}
