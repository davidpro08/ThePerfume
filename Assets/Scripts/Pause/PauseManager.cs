using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPaused;

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

    public void TogglePause(bool setUI = false)
    {
        if (isPaused) Resume();
        else Pause(setUI);
    }

    private void Pause(bool setUI)
    {
        Time.timeScale = 0f;
        isPaused = true;
        if(setUI) pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public bool IsPaused() => isPaused;
}