using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{ 
    // 싱글톤
    public static PauseManager Instance { get; private set; }
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        pauseMenuUI.SetActive(false);
    }

    // PlayerInput에서 Pause 액션 호출 시 연결
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;
        pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public bool IsPaused() => isPaused;
}