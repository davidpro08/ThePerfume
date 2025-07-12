using Unity.VisualScripting;
using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    public Transform playerTransform;
    [SerializeField] public float distanceOfFornt = 1.0f;
    void Update()
    {
        if (playerTransform == null) return;

        Vector2 lookDirection = playerTransform.position;
    }
}
