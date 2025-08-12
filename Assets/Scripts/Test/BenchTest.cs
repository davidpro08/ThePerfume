using UnityEngine;

public class BenchTest : MonoBehaviour
{
    [SerializeField] public ItemData roseItem;
    [SerializeField] public ItemData lavendaItem;
    public InventoryManager inventoryManager;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            bool added = InventoryManager.Instance.AddItem(roseItem, 1);
            Debug.Log("Added Rose" + added);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            bool added = InventoryManager.Instance.AddItem(lavendaItem, 1);
            Debug.Log("Added Lavenda" + added);
        }
    }
}
