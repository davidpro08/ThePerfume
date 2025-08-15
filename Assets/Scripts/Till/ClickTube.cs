using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ClickTube : MonoBehaviour
{
    [SerializeField] private LayerMask InteractableLayer;
    private string tubeFuelName = "TubeFuel";
    private string tubePetalName = "TubePetal";
    private string tubeEssenceName = "TubeEssence";

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"Tube click detect");
            //==================================================================
            // UI가 열려있으면 월드 오브젝트 클릭 무시
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
            {
                Debug.Log($"UI open");
                return;
            }
            //==================================================================

            // ======================================================================
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D[] hitsAll = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, Mathf.Infinity, InteractableLayer);

            if (hitsAll.Length > 0)
            {
                Debug.Log("hitsAll.Lenght>0");


                RaycastHit2D hit2D = hitsAll[0];
                string clickedObjectName = hit2D.collider.gameObject.name;
                Debug.Log($"{clickedObjectName} clicked");

                foreach (var hit in hitsAll)
                {
                    string clickedName = hit.collider.gameObject.name;

                    if (clickedName == tubeFuelName || clickedName == tubePetalName || clickedName == tubeEssenceName)
                    {
                        clickedObjectName = clickedName;
                        hit2D = hit;
                        break;
                    }
                }

                if (clickedObjectName == tubeFuelName)
                {
                    // ======================================================================

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

                    if (TillUIManager.Instance == null)
                    {
                        Debug.Log($"Instance null");
                        return;
                    }

                    // ==================================================
                    if (selectedSlot.itemData.itemName == "Fuel")
                    {
                        Debug.Log($"TubeFuel SpawnItemOnTubeFuel call");
                        TillUIManager.Instance.SpawnItemOnTube(selectedSlot.itemData, 1, TillUIManager.Instance.tubeFuelTransformPos);
                    }
                    else
                    {
                        TillUIManager.Instance.ShowWarningCanvas("No Fuel Item");
                        return;
                    }
                    // ==================================================
                }

                else if (clickedObjectName == tubePetalName)
                {
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

                    if (TillUIManager.Instance == null)
                    {
                        Debug.Log($"Instance null");
                        return;
                    }

                    if (selectedSlot.itemData.itemType == ItemType.Material && selectedSlot.itemData.itemName != "Fuel" && !TillUIManager.Instance.isMakingEssence)
                    {
                        Debug.Log($"Tray SpawnItemOnTubeFuel call");
                        TillUIManager.Instance.SpawnItemOnTube(selectedSlot.itemData, 1, TillUIManager.Instance.tubePetalTransformPos);
                    }
                    else if (TillUIManager.Instance.isMakingEssence)
                    {
                        TillUIManager.Instance.ShowWarningCanvas("exist essence");
                    }
                    else
                    {
                        TillUIManager.Instance.ShowWarningCanvas("No Harvested Crop Item");
                        return;
                    }
                }

                else if (clickedObjectName == tubeEssenceName)
                {
                    if (TillUIManager.Instance == null)
                    {
                        Debug.Log("FlowerManager.Instance == null");
                        return;
                    }
                    TillUIManager.Instance.OnTubeEssenceClicked();
                }
            }
        }
    }

}

