using UnityEngine;

public class TillTest : MonoBehaviour
{
    [SerializeField] public ItemData FuelItem;
    [SerializeField] public ItemData RosePetalItem;
    public InventoryManager inventoryManager;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            bool added = InventoryManager.Instance.AddItem(FuelItem, 1);
            Debug.Log("Added Rose" + added);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            bool added = InventoryManager.Instance.AddItem(RosePetalItem, 1);
            Debug.Log("Added Lavenda" + added);
        }
    }
}
