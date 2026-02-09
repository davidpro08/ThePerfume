using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        if (currentScene == "Main")
        {
            if (name == "Play")
            {
                SaveManager.Instance.ResetGame();
            }
            else if (name == "Load")
            {
                SaveManager.Instance.LoadGame();
            }
        }

        if (currentScene == "lab" && SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        // 로딩 UI를 사용하여 씬 전환
        if (LoadingUIManager.Instance != null)
        {
            Resources.UnloadUnusedAssets();
            LoadingUIManager.Instance.LoadScene(targetSceneName);
        }
        else
        {
            // 로딩 UI가 없으면 일반 로드
            Resources.UnloadUnusedAssets();
            SceneManager.LoadScene(targetSceneName);
        }
    }

    public void MoveToScene(Player player)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Main")
        {
            if (name == "Play")
            {
                SaveManager.Instance.ResetGame();
            }
            else if (name == "Load")
            {
                SaveManager.Instance.LoadGame();
            }

            // 로딩 UI를 사용하여 씬 전환
            if (LoadingUIManager.Instance != null)
            {
                LoadingUIManager.Instance.LoadScene(targetSceneName);
            }
            else
            {
                // 로딩 UI가 없으면 일반 로드
                Resources.UnloadUnusedAssets();
                SceneManager.LoadScene(targetSceneName);
            }
        }
        else
        {
            if (currentScene == "lab" && SaveManager.Instance != null)
                SaveManager.Instance.SaveGame();

            // 로딩 UI를 사용하여 씬 전환
            if (LoadingUIManager.Instance != null)
            {
                Resources.UnloadUnusedAssets();
                LoadingUIManager.Instance.LoadScene(targetSceneName);
            }
            else
            {
                // 로딩 UI가 없으면 일반 로드
                Resources.UnloadUnusedAssets();
                SceneManager.LoadScene(targetSceneName);
            }
        }

        // 씬이 로드된 후 플레이어 위치 설정은 씬 로드 완료 후에 처리되어야 함
        // 이 부분은 씬 로드 후 호출되는 별도의 메서드로 처리하는 것이 좋습니다
        StartCoroutine(SetPlayerPositionAfterLoad(player));
    }

    private System.Collections.IEnumerator SetPlayerPositionAfterLoad(Player player)
    {
        // 씬이 완전히 로드될 때까지 대기
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (player != null)
        {
            player.transform.position = targetPosition;
        }
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
