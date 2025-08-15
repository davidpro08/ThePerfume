using UnityEngine;

public class SceneChangerOnTrigger : MonoBehaviour
{
    [SerializeField] private SceneChanger sceneChanger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Distiller distiller = GetComponent<Distiller>();
        if (sceneChanger != null)
        {
            if (distiller != null)
            {
                sceneChanger.GoToTillScene(distiller.distillerID);
            }
            else sceneChanger.MoveToScene();
        }
        else Debug.LogWarning("SceneChanger ����ȵ�", this);
    }
}
