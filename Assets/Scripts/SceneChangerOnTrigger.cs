using UnityEngine;

public class SceneChangerOnTrigger : MonoBehaviour
{
    [SerializeField] private SceneChanger sceneChanger;

    public Vector2 newPlayerPos;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (sceneChanger != null)
        {
            sceneChanger.MoveToScene();
            other.transform.position = newPlayerPos; // 플레이어 위치 조정
        }
        else Debug.LogWarning("SceneChanger 연결안됨", this);
    }
}
