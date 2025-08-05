using TMPro;
using UnityEngine;
using System.Collections;

public class NpcDialogueManager : MonoBehaviour
{
    [Header("대화창 관련 요소")]
    public GameObject dialogueObject;
    public TextMeshProUGUI dialogueText;
    public bool isActive = false;

    [Header("대화 설정")]
    public float textSpeed = 0.05f;
    public bool useTypewriterEffect = true;

    // 현재 대화 상태
    private DialogueEntry currentDialogue;
    private string currentNpcId;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;

    // 싱글톤 인스턴스
    public static NpcDialogueManager Instance { get; private set; }

    void Awake()
    {
        //이미 인스턴스 있으면 자신 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (dialogueObject != null)
            dialogueObject.SetActive(false);
    }

    /// <summary>
    /// NPC와의 대화 시작
    /// </summary>
    public void StartDialogue(string npcId, string dialogueId = null)
    {
        if (CSVDialogueParser.Instance == null)
        {
            Debug.LogError("CSVDialogueParser가 없습니다!");
            return;
        }

        currentNpcId = npcId;

        // 특정 대화 ID가 없으면 첫 번째 대화 사용
        if (string.IsNullOrEmpty(dialogueId))
        {
            var npcDialogues = CSVDialogueParser.Instance.GetDialoguesByNpcId(npcId);
            if (npcDialogues.Count > 0)
            {
                currentDialogue = npcDialogues[0];
            }
            else
            {
                Debug.LogError($"NPC {npcId}의 대화 데이터를 찾을 수 없습니다!");
                return;
            }
        }
        else
        {
            currentDialogue = CSVDialogueParser.Instance.GetDialogueById(dialogueId);
            if (currentDialogue == null)
            {
                Debug.LogError($"대화 ID {dialogueId}를 찾을 수 없습니다!");
                return;
            }
        }

        ShowDialogue(currentDialogue);
    }

    /// <summary>
    /// 대화창 표시
    /// </summary>
    public void ShowDialogue(DialogueEntry dialogue)
    {
        if (dialogueObject == null || dialogueText == null)
        {
            Debug.LogError("대화창 UI 요소가 설정되지 않았습니다!");
            return;
        }

        currentDialogue = dialogue;
        isActive = true;
        dialogueObject.SetActive(true);

        // 기존 타이핑 효과 중지
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // 타이핑 효과 적용
        if (useTypewriterEffect)
        {
            typewriterCoroutine = StartCoroutine(TypewriterEffect(dialogue.dialogueText));
        }
        else
        {
            dialogueText.text = dialogue.dialogueText;
        }

        // 대화 중일 때만 플레이어 이동 제한 (UI는 계속 작동)
        PauseManager.Instance.PauseForDialogue();
    }

    /// <summary>
    /// 타이핑 효과
    /// </summary>
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// 대화 종료
    /// </summary>
    public void EndDialogue()
    {
        isActive = false;
        currentDialogue = null;
        currentNpcId = null;

        if (dialogueObject != null)
            dialogueObject.SetActive(false);

        // 대화 종료 시 플레이어 이동 재개
        PauseManager.Instance.ResumeFromDialogue();
    }

    // /// <summary>
    // /// 대화창 설정 (기존 호환성 유지)
    // /// </summary>
    // public void SetDialogueText(string text)
    // {
    //     if (dialogueObject == null)
    //     {
    //         Debug.Log($"[{name}] : dialogueObject 요소 연결 필요");
    //         return;
    //     }

    //     if (dialogueText == null)
    //     {
    //         Debug.Log($"[{name}] : dialogueText 요소 연결 필요");
    //         return;
    //     }

    //     // 상태 반전
    //     isActive = !isActive;

    //     // 텍스트 설정하고 열거나 닫는 부분
    //     dialogueText.text = text;
    //     dialogueObject.SetActive(isActive);
    // }

    /// <summary>
    /// 다음 버튼 클릭 (타이핑 효과 스킵)
    /// </summary>
    public void OnNextButtonClicked()
    {
        if (isTyping)
        {
            // 타이핑 효과 스킵
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            dialogueText.text = currentDialogue.dialogueText;
            isTyping = false;
        }
        else
        {
            // 대화 종료
            EndDialogue();
        }
    }
}