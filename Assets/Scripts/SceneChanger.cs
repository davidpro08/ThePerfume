using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("이동한 씬")]
    [SerializeField] private string targetSceneName;

    public void MoveToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("씬 이름이 설정 안됨", this);
            return;
        }

        // 씬 이동마다 농장 상태 저장// 로드
        GameSave save = new GameSave();

        if (FarmSaveService.Instance != null)
        {
            save.farms = FarmSaveService.Instance.CreateFarmSnapshot();
        }

        if (Mixture.Instance != null) save.mixture = Mixture.Instance.CreateSnapshot();

        if (InventoryManager.Instance != null) save.inventory = InventoryManager.Instance.CreateSnapshot();

        foreach (Distiller d in FindObjectsByType<Distiller>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) save.distillers.Add(d.SaveSnapshot());

        SaveManager.Save(save);

        SceneManager.LoadScene(targetSceneName);
    }
}
