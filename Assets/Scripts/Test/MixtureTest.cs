using UnityEngine;

public class MixtureTest : MonoBehaviour
{
    [SerializeField] public ItemData roseEssenceData;
    public InventoryManager inventoryManager;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            bool added = InventoryManager.Instance.AddItem(roseEssenceData, 1);
            Debug.Log("Added roseEssenceData" + added);
        }
    }
}
