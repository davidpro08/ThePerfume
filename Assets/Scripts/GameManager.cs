using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("씬전환에 남길 오브젝트")]
    public GameObject[] persistentObjects;

    private void Awake()
    {
        if(instance != null)
        {
            CleanUpAndDestroy();
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            MarkPersistentObjects();
        }
    }

    private void MarkPersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }

    private void CleanUpAndDestroy()
    {
        foreach(GameObject obj in persistentObjects)
        {
            Destroy(obj);
        } 
    }
}
