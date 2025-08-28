using UnityEngine;

public class SceneChangerOnTrigger : MonoBehaviour
{
    [SerializeField] private SceneChanger sceneChanger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (sceneChanger != null)
        {
            sceneChanger.MoveToScene();
        }
        else Debug.LogWarning("SceneChanger ¿¬°á¾ÈµÊ", this);
    }
}
