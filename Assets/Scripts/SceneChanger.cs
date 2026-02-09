using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour, IInteract
{
    [Header("이동한 씬")]
    [SerializeField] private string targetSceneName;
    [Header("이동할 위치")]
    [SerializeField] public Vector2 targetPosition;
    [Header("이동 방식")]
    [SerializeField] public bool isTrigger;

    public void MoveToScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // if (currentScene == "Main")
        // {
        //     if (name == "Play")
        //     {
        //         SaveManager.Instance.ResetGame();
        //     }
        //     else if (name == "Load")
        //     {
        //         SaveManager.Instance.LoadGame();
        //     }
        // }

        if (currentScene == "lab" && SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(targetSceneName);
    }

    public void MoveToScene(Player player)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // if (currentScene == "Main")
        // {
        //     if (name == "Play")
        //     {
        //         SaveManager.Instance.ResetGame();
        //     }
        //     else if (name == "Load")
        //     {
        //         SaveManager.Instance.LoadGame();
        //     }
        // }

        if (currentScene == "lab" && SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(targetSceneName);

        player.transform.position = targetPosition;
    }

    public void Interact(Player player)
    {
        if (CanInteract(player)) MoveToScene(player);
    }

    public bool CanInteract(Player player)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("씬 이름이 설정 안됨", this);
            return false;
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTrigger)
        {
            Debug.Log("트리거 이동 방식이 아님");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Debug.Log("충돌한 객체가 플레이어가 아님");
            return;
        }
        Player player = other.GetComponent<Player>();
        Interact(player);
    }
}
