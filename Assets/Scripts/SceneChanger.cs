using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("이동한 씬")]
    [SerializeField] private string targetSceneName;

    public static SceneChanger Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene scene)
    {
        SaveGameState();
    }

    public void SaveGameState()
    {
        // 씬 이동마다 농장 상태 저장
        GameSave save = new GameSave();

        if (FarmSaveService.Instance != null)
        {
            var snapshot = FarmSaveService.Instance.CreateFarmSnapshot();
            Debug.Log($"Farm 저장 갯수 : {snapshot.Count}");
            save.farms = snapshot;
        }

        if (Mixture.Instance != null) save.mixture = MixtureSaveService.Instance.CreateMixtureSnapshot();

        if (InventoryManager.Instance != null) save.inventory = InventoryManager.Instance.CreateSnapshot();

        if (Distiller.Instance != null) save.distillers = DistillerSaveService.Instance.CreateDistillerSapshots();

        SaveManager.Save(save);
    }

    public void MoveToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("씬 이름이 설정 안됨", this);
            return;
        }

        SaveGameState();
        SceneManager.LoadScene(targetSceneName);
    }
}
