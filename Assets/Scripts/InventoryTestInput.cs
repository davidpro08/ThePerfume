using UnityEngine;

public class InventoryTestInput : MonoBehaviour
{
    public ItemData item1;
    public ItemData item2;
    public ItemData item3;

    public InventoryManager inventoryManager;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            bool added = inventoryManager.AddItem(item1, 1);
            Debug.Log("Added stackable" + added);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            bool added = inventoryManager.AddItem(item2, 1);
            Debug.Log("Added stackable" + added);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            bool added = inventoryManager.AddItem(item3, 5);
            Debug.Log("Added stackable" + added);
        }

    }
}
