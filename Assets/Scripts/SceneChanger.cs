using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("이동한 씬")]
    [SerializeField] private string targetSceneName;

    public string currentDistillerID = null;
    public static SceneChanger Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MoveToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("씬 이름이 설정 안됨", this);
            return;
        }
        currentDistillerID = null;

        Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(targetSceneName);
    }

    public void GoToTillScene(string id)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("씬 이름이 설정 안됨", this);
            return;
        }
        currentDistillerID = id;
        SceneManager.LoadScene(targetSceneName);
    }

    public void ResetDistillerID()
    {
        currentDistillerID = null;
    }
}
