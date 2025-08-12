using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ItemOnTrayClick : MonoBehaviour
{
    public ItemData ItemData;


    // ===============================================
    // bench 씬에서만 작동하도록 제한해놓음 >> 필요 없어짐
    // private string targetSceneName = "bench";

    // void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnScneneLoaded;
    // }
    // void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnScneneLoaded;
    // }
    // private void OnScneneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     CheckCurrentScene();
    // }
    // private void CheckCurrentScene()
    // {
    //     string currentScene = SceneManager.GetActiveScene().name;
    //     Debug.Log($"[ItemOnTrayClick] 현재 씬 = {currentScene}");

    //     Collider2D col = GetComponent<Collider2D>();
    //     if (col == null) return;

    //     if (currentScene == targetSceneName)
    //     {
    //         col.enabled = true;
    //         Debug.Log($"[{name}] Bench 씬 > Collider 활성화");
    //     }
    //     else
    //     {
    //         col.enabled = false;
    //         Debug.Log($"[{name}] Bench 씬 > Collider 비활성화");
    //     }
    // }
    // ===============================================

    public void OnCropClicked()
    {
        Debug.Log($"{gameObject.name} 클릭");
        TrayClick.cropClicked = false;

        Debug.Log($"FlowerManager.Instance == {FlowerManager.Instance}");

        if (FlowerManager.Instance != null)
        {
            FlowerManager.Instance.StartHandling(ItemData, this);
            BenchInventoryUIManager.Instance.RemoveSpawnedItemd(this.gameObject);
            Debug.Log($"[ItemOnTrayClick] {this.gameObject.name}, {this.gameObject != null}");
        }
        else
        {
            Debug.Log($"FLowerManager.Instance == null");
        }
    }
}
