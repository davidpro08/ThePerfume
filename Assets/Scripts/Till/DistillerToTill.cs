using UnityEngine;
using UnityEngine.SceneManagement;

public class DistillerToTill : MonoBehaviour
{
    [SerializeField] string distillerID;
    [SerializeField] string tillSceneName = "distiller";
    [SerializeField] private bool isPlayerInTrigger = false;

    void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.Space))
        {
            EnterTill();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Debug.Log("스페이스바로 이동 가능");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    public void EnterTill()
    {
        if (string.IsNullOrEmpty(tillSceneName))
        {
            Debug.LogWarning("이동할 씬 이름 설정 안됨", this);
        }
        if (GameContext.Instance == null)
        {
            GameContext context = new GameObject("GameContext").AddComponent<GameContext>();
        }
        GameContext.Instance.SelectDistiller(distillerID);

        // 씬 이동마다 농장 상태 저장
        GameSave save = new GameSave();

        if (FarmSaveService.Instance != null)
        {
            var snapshot = FarmSaveService.Instance.CreateFarmSnapshot();
            Debug.Log($"Farm 저장 갯수 : {snapshot.Count}");
            save.farms = snapshot;
        }

        if (Mixture.Instance != null) save.mixture = Mixture.Instance.CreateSnapshot();

        if (InventoryManager.Instance != null) save.inventory = InventoryManager.Instance.CreateSnapshot();

        foreach (Distiller d in FindObjectsByType<Distiller>(FindObjectsInactive.Include, FindObjectsSortMode.None)) save.distillers.Add(d.SaveSnapshot());

        SaveManager.Save(save);

        SceneManager.LoadScene(tillSceneName);
    }
}
