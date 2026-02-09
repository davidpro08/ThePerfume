using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class NoticeUIManager : MonoBehaviour
{
    public static NoticeUIManager Instance { get; private set; }
    [Header("안내창UI")]
    [SerializeField] private GameObject noticeCanvas;
    [SerializeField] private TextMeshProUGUI noticeMessageText;
    [SerializeField] private CanvasGroup noticeCanvasGroup;

    private Queue<string> mQueue = new Queue<string>();
    private bool isDisplayNotice = false;
    [Header("option")]
    [SerializeField] private float displayDuration = 1.0f;
    [SerializeField] private float fadeDuration = 0.3f;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (noticeCanvas != null)
        {
            if (noticeCanvasGroup != null)
            {
                noticeCanvas.SetActive(false);
                noticeCanvasGroup.alpha = 1.0f;
            }
        }
    }

    // 경고창 표시
    public void ShowNoticeCanvas(string message)
    {
        mQueue.Enqueue(message);
        if (!isDisplayNotice)
        {
            StartCoroutine(DisplayNoticeCoroutine());
        }
    }

    private IEnumerator DisplayNoticeCoroutine()
    {
        isDisplayNotice = true;
        if (noticeCanvas != null) noticeCanvasGroup.alpha = 1.0f;
        noticeCanvas.SetActive(true);

        while (mQueue.Count > 0)
        {
            string nextMessage = mQueue.Dequeue();
            noticeMessageText.text = nextMessage;

            yield return new WaitForSeconds(displayDuration);

            if (mQueue.Count == 0)
            {
                yield return StartCoroutine(FadeOut());
            }
        }
        noticeCanvas.SetActive(false);
        isDisplayNotice = false;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = noticeCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            if (noticeCanvasGroup != null)
            {
                float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
                noticeCanvasGroup.alpha = newAlpha;
            }
            yield return null;
        }
        if (noticeCanvasGroup != null) noticeCanvasGroup.alpha = 0f;
    }
}
