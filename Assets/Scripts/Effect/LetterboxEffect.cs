using UnityEngine;

public class LetterboxEffect : MonoBehaviour
{
    public static LetterboxEffect instance { get; private set; }
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

    // Letterbox 애니메이션 재생
    public void SetLetterbox(bool enable)
    {
        if (animator == null)
        {
            Debug.LogError("Animator component is missing.");
            return;
        }

        animator.SetBool("isOpen", enable);
    }
}
