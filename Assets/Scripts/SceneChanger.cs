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

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "lab" && SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(targetSceneName);
    }
}
