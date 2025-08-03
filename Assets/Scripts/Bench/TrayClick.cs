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
            //==================================================================
            // UI가 열려도 월드 오브젝트 클릭 무시가 안됨
            // UI가 열려있으면 월드 오브젝트 클릭 무시
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
            {
                Debug.Log($"UI open");
                return;
            }
            //==================================================================

            // ======================================================================
            // Tray 클릭 시 손질 UI랑 프리팹 소환이 동시에 되는 현상 수정
            // 자식 프리팹 있는지 확인
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D[] hitsAll = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, Mathf.Infinity, InteractableLayer);

            if (hitsAll.Length > 0)
            {
                foreach (var hit in hitsAll)
                {
                    Debug.Log($"{hit.collider.gameObject.name} 인식");
                }

                RaycastHit2D hit2D = hitsAll[0];
                string clickedObjectName = hit2D.collider.gameObject.name;
                Debug.Log($"{clickedObjectName} clicked");

                if (clickedObjectName == trayName)
                {

                    Transform trayTranform = hit2D.collider.transform;

                    foreach (var hit in hitsAll)
                    {
                        Debug.Log("Tray 자식 확인 중...");
                        if (hit.collider.transform != trayTranform && hit.collider.transform.IsChildOf(trayTranform))
                        {
                            Debug.Log("Tray 자식 클릭");
                            return;
                        }
                    }
                    // ======================================================================

                    if (BenchInventoryUIManager.Instance == null)
                    {
                        Debug.Log($"Instance null");
                        return;
                    }

                    // 현재 선택된 슬롯의 아이템 가져오기
                    // 범위 밖 인덱스 오류로 인해 안전장치 추가
                    int selectedRealIndex = InventoryManager.Instance.SelectedSlotIndex;
                    if (selectedRealIndex < 0 || selectedRealIndex >= InventoryManager.Instance.itemSlots.Count)
                    {
                        Debug.Log($"[{name}] : 선택된 슬롯 인덱스가 유효 범위를 넘어감");
                        return;
                    }
                    ItemSlot selectedSlot = InventoryManager.Instance.itemSlots[selectedRealIndex];
                    if (ReferenceEquals(selectedSlot.itemData, null))
                    {
                        Debug.Log($"[{name}] : 아이템의 정보가 없음");
                        return;
                    }

                    // ==================================================
                    if (selectedSlot.itemData.itemType == ItemType.Crop)
                    {
                        Debug.Log($"Tray SpawnItemOnTray call");
                        BenchInventoryUIManager.Instance.SpawnItemOnTray(selectedSlot.itemData, 1);
                    }
                    else
                    {
                        return;
                    }
                    // ==================================================
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
