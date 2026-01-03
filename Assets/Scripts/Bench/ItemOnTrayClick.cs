using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ItemOnTrayClick : MonoBehaviour
{
    public ItemData ItemData;

    public void OnCropClicked()
    {
        Debug.Log($"{gameObject.name} 클릭");
        TrayClick.cropClicked = true;

        Debug.Log($"FlowerManager.Instance == {FlowerManager.Instance}");

        if (FlowerManager.Instance != null)
        {
            FlowerManager.Instance.StartHandling(ItemData, this);
            BenchUIManager.Instance.RemoveSpawnedItem(this.gameObject);
            Debug.Log($"[ItemOnTrayClick] {this.gameObject.name}, {this.gameObject != null}");
        }
        else
        {
            Debug.Log($"FLowerManager.Instance == null");
        }

        TrayClick.cropClicked = false;
    }
}
