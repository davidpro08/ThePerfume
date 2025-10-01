using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerOnTrigger : MonoBehaviour
{
    [SerializeField] private SceneChanger sceneChanger;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (sceneChanger == null)
        {
            Debug.Log("씬 체인저 없음");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Debug.Log("충돌한 객체가 플레이어가 아님");
            return;
        }
        Player player = other.GetComponent<Player>();
        sceneChanger.MoveToScene(player);
    }
}
