using UnityEngine;

public class LightingEffect : MonoBehaviour
{
    public static LightingEffect instance { get; private set; }
    private Animator animator;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        animator = GetComponent<Animator>();
    }

    public void SetLighting(bool enable)
    {
        if (animator == null)
        {
            Debug.LogError("Animator component is missing.");
            return;
        }

        animator.SetBool("isLight", enable);
    }
}
