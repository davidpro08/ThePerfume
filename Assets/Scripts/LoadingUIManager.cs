using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingUIManager : MonoBehaviour
{
    private static LoadingUIManager _instance;

    [Header("로딩 UI 컴포넌트")]
    [SerializeField]
    private CanvasGroup _loadingCanvas;
    [SerializeField]
    private Slider _progressSlider;
    [SerializeField]
    private TextMeshProUGUI _loadingText;
    [SerializeField]
    private TextMeshProUGUI _progressText;

    [Header("로딩 설정")]
    [SerializeField]
    private float _fadeSpeed = 0.1f;
    [SerializeField]
    private float _minDisplayTime = 1.0f; // 최소 1초 로딩 시간 보장
    [SerializeField]
    private string[] _loadingMessages =
    {
        "로딩 중...",
        "씬을 불러오는 중...",
        "잠시만 기다려주세요..."
    };

    private string _loadSceneName;
    private bool _isLoading = false;
    private Canvas _canvas;

    public static LoadingUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = FindObjectOfType<LoadingUIManager>();

                if (obj != null)
                    _instance = obj;
                else
                    _instance = Create();
            }

            return _instance;
        }
    }

    private static LoadingUIManager Create()
    {
        // Resources 폴더에서 LoadingUI 프리팹 찾기 (UI 폴더 또는 루트)
        var prefab = Resources.Load<GameObject>("UI/LoadingUI") ?? Resources.Load<GameObject>("LoadingUI");

        if (prefab == null)
        {
            Debug.LogError("LoadingUI 프리팹을 Resources 폴더에서 찾을 수 없습니다! " +
                          "Assets/Prefab/UI/LoadingUI.prefab을 Assets/Resources/UI/LoadingUI.prefab으로 복사해주세요.");
            return null;
        }

        var instance = Instantiate(prefab);
        var manager = instance.GetComponent<LoadingUIManager>();
        if (manager == null)
        {
            Debug.LogError("LoadingUI 프리팹에 LoadingUIManager 컴포넌트가 없습니다!");
        }

        return manager;
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // Canvas 컴포넌트 찾기 (CanvasGroup이 있는 GameObject 또는 부모에서)
        _canvas = GetComponent<Canvas>();
        if (_canvas == null && _loadingCanvas != null)
        {
            _canvas = _loadingCanvas.GetComponent<Canvas>();
        }
        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        // Canvas의 sortingOrder를 최대값으로 설정하여 모든 UI 위에 표시
        if (_canvas != null)
        {
            _canvas.sortingOrder = 32767; // 최대값으로 설정
            _canvas.overrideSorting = true; // 다른 Canvas의 설정을 무시하고 이 값을 사용
        }

        // 초기 상태 설정 - gameObject는 활성화 상태로 유지하고 CanvasGroup만 제어
        if (_loadingCanvas != null)
        {
            _loadingCanvas.alpha = 0f;
            _loadingCanvas.blocksRaycasts = false;
            _loadingCanvas.interactable = false;
        }

        // Canvas 자체는 활성화 상태로 유지 (컴포넌트 초기화를 위해)
        // 대신 CanvasGroup으로 가시성과 상호작용을 제어
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("씬 이름이 비어있습니다!");
            return;
        }

        if (_isLoading)
        {
            Debug.LogWarning("이미 로딩 중입니다!");
            return;
        }

        _loadSceneName = sceneName;
        _isLoading = true;

        // gameObject가 비활성화되어 있을 수 있으므로 활성화
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // Canvas의 sortingOrder를 다시 확인하여 최상위로 설정
        if (_canvas != null)
        {
            _canvas.sortingOrder = 32767;
            _canvas.overrideSorting = true;
        }
        else
        {
            // Canvas를 다시 찾기 시도
            _canvas = GetComponent<Canvas>();
            if (_canvas == null && _loadingCanvas != null)
            {
                _canvas = _loadingCanvas.GetComponent<Canvas>();
            }
            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
            }
            if (_canvas != null)
            {
                _canvas.sortingOrder = 32767;
                _canvas.overrideSorting = true;
            }
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(LoadSceneProgress());
    }

    private IEnumerator LoadSceneProgress()
    {
        // 초기화
        if (_progressSlider != null)
        {
            _progressSlider.value = 0.0f;
            _progressSlider.minValue = 0.0f;
            _progressSlider.maxValue = 1.0f;
        }

        if (_progressText != null)
            _progressText.text = "0%";

        // 페이드 인
        yield return StartCoroutine(Fade(true));

        // 로딩 시작 시간 기록 (최소 1초 보장을 위해)
        float loadStartTime = Time.time;
        int messageIndex = 0;

        // 로딩 메시지 업데이트
        if (_loadingText != null && _loadingMessages.Length > 0)
        {
            _loadingText.text = _loadingMessages[messageIndex];
        }

        // 씬 로드 시작
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_loadSceneName);
        if (asyncOperation == null)
        {
            Debug.LogError($"씬 '{_loadSceneName}'을 찾을 수 없습니다!");

            // 에러가 발생해도 최소 1초는 표시
            float elapsedTime = Time.time - loadStartTime;
            if (elapsedTime < _minDisplayTime)
            {
                yield return new WaitForSeconds(_minDisplayTime - elapsedTime);
            }

            yield return StartCoroutine(Fade(false));
            _isLoading = false;
            yield break;
        }

        asyncOperation.allowSceneActivation = false;

        float timer = 0.0f;
        float smoothProgress = 0.0f;

        while (!asyncOperation.isDone)
        {
            yield return null;

            // 진행률 업데이트
            float progress = asyncOperation.progress;

            if (progress < 0.9f)
            {
                smoothProgress = progress;

                if (_progressSlider != null)
                    _progressSlider.value = smoothProgress;

                if (_progressText != null)
                    _progressText.text = $"{Mathf.RoundToInt(smoothProgress * 100)}%";

                // 로딩 메시지 변경 (선택적)
                if (_loadingText != null && _loadingMessages.Length > 1)
                {
                    float messageChangeTime = Time.time - loadStartTime;
                    int newIndex = Mathf.FloorToInt(messageChangeTime / 1.0f) % _loadingMessages.Length;
                    if (newIndex != messageIndex)
                    {
                        messageIndex = newIndex;
                        _loadingText.text = _loadingMessages[messageIndex];
                    }
                }
            }
            else
            {
                // 90% 이상일 때 부드럽게 100%까지
                timer += Time.unscaledDeltaTime;
                smoothProgress = Mathf.Lerp(0.9f, 1.0f, timer);

                if (_progressSlider != null)
                    _progressSlider.value = smoothProgress;

                if (_progressText != null)
                    _progressText.text = $"{Mathf.RoundToInt(smoothProgress * 100)}%";

                if (smoothProgress >= 1.0f)
                {
                    // 최소 표시 시간 확인 (로딩할 내용이 없어도 1초 이상 보장)
                    float elapsedTime = Time.time - loadStartTime;
                    if (elapsedTime < _minDisplayTime)
                    {
                        // 남은 시간 동안 100% 유지
                        yield return new WaitForSeconds(_minDisplayTime - elapsedTime);
                    }

                    asyncOperation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    private IEnumerator Fade(bool isFadeIn)
    {
        if (_loadingCanvas == null)
            yield break;

        float timer = 0.0f;
        float targetAlpha = isFadeIn ? 1.0f : 0.0f;
        float startAlpha = _loadingCanvas.alpha;

        _loadingCanvas.blocksRaycasts = isFadeIn;

        while (timer <= 1.0f)
        {
            yield return null;

            timer += Time.unscaledDeltaTime * _fadeSpeed;
            _loadingCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer);
        }

        _loadingCanvas.alpha = targetAlpha;

        if (!isFadeIn)
        {
            _loadingCanvas.blocksRaycasts = false;
            _loadingCanvas.interactable = false;
            _isLoading = false;
            // gameObject는 활성화 상태로 유지 (다음 로딩을 위해)
            // CanvasGroup의 alpha가 0이므로 보이지 않음
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == _loadSceneName)
        {
            StartCoroutine(Fade(false));
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
