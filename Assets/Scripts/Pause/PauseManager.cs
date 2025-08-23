using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPaused;
    private bool isDialoguePaused = false;

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
        
        SoundManager.Instance.PlaySFX(SFXType.Pause);
    }

    private void Pause(bool setUI)
    {
        Time.timeScale = 0f;
        isPaused = true;
        if (setUI) pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }

    /// <summary>
    /// 대화 중일 때만 플레이어 이동을 제한하고 UI는 계속 작동하게 함
    /// </summary>
    public void PauseForDialogue()
    {
        isDialoguePaused = true;
        // Time.timeScale은 건드리지 않아서 UI 애니메이션 등은 계속 작동
    }

    /// <summary>
    /// 대화 종료 시 플레이어 이동 재개
    /// </summary>
    public void ResumeFromDialogue()
    {
        isDialoguePaused = false;
    }

    /// <summary>
    /// 대화 중인지 확인
    /// </summary>
    public bool IsDialoguePaused() => isDialoguePaused;

    /// <summary>
    /// 전체 게임이 일시정지된 상태인지 확인
    /// </summary>
    public bool IsPaused() => isPaused;

    /// <summary>
    /// 플레이어 이동이 제한된 상태인지 확인 (대화 중이거나 일시정지)
    /// </summary>
    public bool IsPlayerMovementBlocked() => isPaused || isDialoguePaused;
}