using UnityEngine;
using UnityEngine.InputSystem;

public class BenchManager : MonoBehaviour
{
    private bool isOpenInventory = false;
    [SerializeField] public InventoryUIManager inventoryUIManager;

    void OnInteract(InputValue value)
    {
        isOpenInventory = !isOpenInventory;

        if (inventoryUIManager != null)
        {
            inventoryUIManager.ToggleFullInventory();
        }
        else
        {
            Debug.LogWarning("InventoryUIManager가 Player 스크립트에 연결되지 않았습니다.");
        }
    }
    
}
