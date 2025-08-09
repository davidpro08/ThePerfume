using System;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NPC;
using Unity.VisualScripting;
using UnityEngine.UI;

public class NpcDialogueManager : MonoBehaviour
{
    [Header("대화창 관련 요소")] public GameObject dialogueObject;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButton; // 다음 버튼
    public bool isActive = false;

    [Header("선택지 관련 요소")] public GameObject choiceButtonPrefab; // 선택지 버튼 프리팹
    public Transform choiceButtonContainer; // 선택지 버튼들을 담을 컨테이너
    private List<ChoiceButton> choiceButtons = new List<ChoiceButton>();

    [Header("대화 설정")] public float textSpeed = 0.05f;
    public bool useTypewriterEffect = true;

    // 현재 대화 상태
    [SerializeField]private DialogueEntry currentDialogue;
    private string currentNpcId;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;

    // 싱글톤 인스턴스
    public static NpcDialogueManager Instance { get; private set; }

    void Awake()
    {
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

        if (string.IsNullOrEmpty(dialogueId))
        {
            var npcDialogues = CSVDialogueParser.Instance.GetDialoguesByNpcId(npcId);
            currentDialogue = (npcDialogues.Count > 0) ? npcDialogues[0] : null;
        }
        else
        {
            currentDialogue = CSVDialogueParser.Instance.GetDialogueById(dialogueId);
        }

        if (currentDialogue == null)
        {
            Debug.LogError($"대화 데이터를 찾을 수 없습니다! NPC ID: {npcId}, Dialogue ID: {dialogueId}");
            return;
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

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        if (useTypewriterEffect)
        {
            typewriterCoroutine = StartCoroutine(TypewriterEffect(dialogue.dialogueText));
        }
        else
        {
            dialogueText.text = dialogue.dialogueText;

            // 선택지가 있는 대화라면 선택지 나오게 함
            if (IsHaveChoices()) DisplayChoices();
        }

        PauseManager.Instance.PauseForDialogue();
    }

    /// <summary>
    /// 타이핑 효과
    /// </summary>
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        // 선택지가 있는 대화라면 선택지 나오게 함
        if (IsHaveChoices()) DisplayChoices();
    }

    private bool IsHaveChoices()
    {
        if (currentDialogue == null)
        {
            Debug.LogError("현재 진행하는 대화가 없습니다.");
            return false;
        }

        if (currentDialogue.choices == null)
        {
            Debug.Log("선택지 대화가 아닙니다.");
            return false;
        }

        if (!nextButton)
        {
            Debug.LogError("nextButton 프리팹을 연결해주세요!");
            return false;
        }

        int choicesLength = currentDialogue.choices.Length;

        if (choicesLength == 0)
        {
            Debug.Log("선택지가 없습니다.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 선택지 표시
    /// </summary>
    private void DisplayChoices()
    {
        int choicesLength = currentDialogue.choices.Length;
        int nextDialogueLength = currentDialogue.nextDialogueIds.Length;

        if (choicesLength > nextDialogueLength)
        {
            Debug.LogError("선택지와 다음 선택지 ID 개수가 잘못되었습니다. 아마 데이터 문제이거나 파싱 문제일 겁니다. 선택지 개수는 다음 문장 개수보다 클 수 없습니다.");
        }

        // 오브젝트 풀링을 이용해서 리소스 최소화
        while (choiceButtons.Count < choicesLength)
        {
            Debug.Log("선택지 개수가 부족합니다. 더 생성합니다.");
            MakeMoreChoiceButtons();
        }

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (i < choicesLength)
            {
                ChoiceButton choiceButton = choiceButtons[i];
                choiceButton.Setting(currentDialogue.choices[i], currentDialogue.nextDialogueIds[i]);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 선택지 버튼 더 만들기(풀링 방식)
    /// </summary>
    private void MakeMoreChoiceButtons()
    {
        GameObject button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
        button.SetActive(false);

        ChoiceButton choiceButton = button.GetComponent<ChoiceButton>();
        choiceButton.button.onClick.AddListener(() => OnChoiceSelected(choiceButton.nextDialogueId));
        choiceButtons.Add(choiceButton);
    }

    /// <summary>
    /// 선택지 클릭 시 호출
    /// </summary>
    public void OnChoiceSelected(string nextDialogueId)
    {
        foreach (ChoiceButton choiceButton in choiceButtons)
        {
            choiceButton.OtherSelected();
        }
        
        StartDialogue(currentNpcId, nextDialogueId);
    }

    /// <summary>
    /// 다음 버튼 클릭 (선택지 없을 때)
    /// </summary>
    public void OnNextButtonClicked()
    {
        if (isTyping)
        {
            StopCoroutine(typewriterCoroutine);
            dialogueText.text = currentDialogue.dialogueText;
            isTyping = false;
            DisplayChoices();
            return;
        }
        
        // 선택지가 있을 때는 다음 버튼 무시
        if (currentDialogue.choices.Length != 0)
        {
            return;
        }

        if (currentDialogue.isEndDialogue || currentDialogue.nextDialogueIds.Length == 0)
        {
            EndDialogue();
        }
        else
        {
            StartDialogue(currentNpcId, currentDialogue.nextDialogueIds[0]);
        }
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

        PauseManager.Instance.ResumeFromDialogue();
    }
}