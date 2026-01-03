using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TrayClick : MonoBehaviour
{
    [SerializeField] private LayerMask InteractableLayer;
    private string trayName = "Tray";
    private string bowlName = "Bowl";
    public static bool cropClicked = false;



    void Update()
    {
        // if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Tray/bowl click detect");
            //==================================================================
            // UI가 열려도 월드 오브젝트 클릭 무시가 안됨
            // UI가 열려있으면 월드 오브젝트 클릭 무시
            Debug.Log($"InventoryUIManager.isFullInventoryOpen : {InventoryUIManager.isFullInventoryOpen}");
            if (InventoryUIManager.isFullInventoryOpen || FlowerManager.Instance.blockingCanvasOpen || cropClicked)
            {
                if (InventoryUIManager.isFullInventoryOpen)
                {
                    Debug.Log($"Full Inventory open");
                }
                else if (FlowerManager.Instance.blockingCanvasOpen)
                {
                    Debug.Log($"FlowerManager blocking canvas open");
                }
                else if (cropClicked)
                {
                    Debug.Log($"Crop clicked");
                }
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1) || FlowerManager.Instance.isHandling)
                {
                    Debug.Log($"UI open");
                    return;
                }
            }

            //==================================================================

            // ======================================================================
            // Tray 클릭 시 손질 UI랑 프리팹 소환이 동시에 되는 현상 수정
            // 자식 프리팹 있는지 확인
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D[] hitsAll = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, Mathf.Infinity, InteractableLayer);

            // 1. 작물 인식
            foreach (var hit in hitsAll)
            {
                ItemOnTrayClick itemOnTrayClick = hit.collider.GetComponent<ItemOnTrayClick>();
                if (itemOnTrayClick != null)
                {
                    Debug.Log("Crop detected");
                    itemOnTrayClick.OnCropClicked();
                    return;
                }
            }

            // 2. Tray/bowl 처리
            foreach (var hit in hitsAll)
            {
                if (hit.collider.gameObject.name == trayName)
                {
                    Debug.Log("Tray clicked");
                    if (BenchUIManager.Instance == null)
                    {
                        Debug.Log($"Instance null");
                        return;
                    }

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

                    if (selectedSlot.itemData.itemType == ItemType.Crop)
                    {
                        Debug.Log($"Tray SpawnItemOnTray call");
                        BenchUIManager.Instance.SpawnItemOnTray(selectedSlot.itemData, 1);
                        InventoryManager.Instance.RemoveItem(selectedSlot.itemData, 1);
                    }
                    else
                    {
                        NoticeUIManager.Instance.ShowNoticeCanvas("No Crop Item");
                    }
                    return;
                }

                else if (hit.collider.gameObject.name == bowlName)
                {
                    Debug.Log("Bowl clicked");
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
