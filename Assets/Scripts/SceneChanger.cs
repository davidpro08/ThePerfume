using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("이동한 씬")]
    [SerializeField] private string targetSceneName;

    public string currentDistillerID = null;

    public void MoveToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("씬 이름이 설정 안됨", this);
            return;
        }

        Resources.UnloadUnusedAssets();
        if (targetSceneName == "distiller")
        {

        }
        SceneManager.LoadScene(targetSceneName);
    }

    public void GoToTillScene(string id)
    {
        currentDistillerID = id;
    }

    public void ResetDistillerID()
    {
        currentDistillerID = null;
    }
}
