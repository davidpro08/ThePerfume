using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 튜토리얼의 각 단계를 정의하는 구조체
public struct TutorialStep
{
    public string triggerId; // 이 단계를 활성화시키는 대화 ID
    public Func<bool> check; // 완료 조건을 검사하는 델리게이트
    public string nextDialogueId; // 완료 후 보여줄 다음 대화 ID
    public bool isCompleted; // 완료 여부
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public Npc guide; // 가이드 NPC (인스펙터에서 할당)

    private List<TutorialStep> tutorialSteps;
    private TutorialStep? currentStep;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeTutorialSteps();
    }

    private void OnEnable()
    {
        NpcDialogueManager.OnDialogueEnd += HandleDialogueEnd;
    }

    private void OnDisable()
    {
        NpcDialogueManager.OnDialogueEnd -= HandleDialogueEnd;
    }

    void Start()
    {
        // TODO: 실제 저장된 데이터에서 튜토리얼 완료 여부 확인
        bool hasCompletedTutorial = false; 
        if (!hasCompletedTutorial)
        {
            StartCoroutine(StartTutorialSequence());
        }
    }

    void Update()
    {
        if (currentStep.HasValue && !currentStep.Value.isCompleted)
        {
            // 현재 단계의 완료 조건을 확인
            if (currentStep.Value.check())
            {
                CompleteStep(currentStep.Value);
            }
        }
    }

    // 튜토리얼 시작
    private IEnumerator StartTutorialSequence()
    {
        yield return new WaitForSeconds(1f); // 게임 시작 후 잠시 대기
        NpcDialogueManager.Instance.StartDialogue(guide, "narration_001_001");
    }

    // 튜토리얼 단계들을 초기화
    private void InitializeTutorialSteps()
    {
        tutorialSteps = new List<TutorialStep>
        {
            new TutorialStep { triggerId = "narration_001_003", check = CheckForTilledSoil, nextDialogueId = "narration_001_004" },
            new TutorialStep { triggerId = "narration_001_004", check = CheckForWateredSoil, nextDialogueId = "narration_001_005" },
            new TutorialStep { triggerId = "narration_001_005", check = CheckForSeededSoil, nextDialogueId = "narration_001_006" },
            new TutorialStep { triggerId = "narration_001_009", check = CheckBenchInteraction, nextDialogueId = "narration_001_010" },
            // ... 여기에 다른 튜토리얼 단계들을 추가 ...
        };
    }

    // NpcDialogueManager에서 대화가 끝났을 때 호출될 핸들러
    private void HandleDialogueEnd(string dialogueId)
    {
        // 끝난 대화 ID에 해당하는 튜토리얼 단계를 찾음
        var stepToStart = tutorialSteps.FirstOrDefault(step => step.triggerId == dialogueId && !step.isCompleted);

        if (!string.IsNullOrEmpty(stepToStart.triggerId))
        {
            Debug.Log($"튜토리얼 단계 시작: {dialogueId}에 의해 트리거됨.");
            currentStep = stepToStart;
        }
    }

    // 현재 단계를 완료 처리
    private void CompleteStep(TutorialStep step)
    {
        Debug.Log($"튜토리얼 단계 완료: {step.triggerId}");

        // 단계 완료로 표시
        int stepIndex = tutorialSteps.FindIndex(s => s.triggerId == step.triggerId);
        var completedStep = tutorialSteps[stepIndex];
        completedStep.isCompleted = true;
        tutorialSteps[stepIndex] = completedStep;

        currentStep = null;

        // 다음 대화 시작
        if (!string.IsNullOrEmpty(step.nextDialogueId))
        {
            NpcDialogueManager.Instance.StartDialogue(guide, step.nextDialogueId);
        }
    }

    #region === 조건 확인 메소드들 ===
    // 이 메소드들은 실제 프로젝트의 구조에 맞게 수정해야 합니다.

    private bool CheckForTilledSoil()
    {
        // Farm 스크립트가 흙 설치 시 생성된다고 가정합니다.
        var farmTile = FindObjectOfType<Farm>(); 
        if (farmTile != null)
        {
            Debug.Log("흙 설치 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForWateredSoil()
    {
        // Farm 스크립트의 isWatered 상태를 확인합니다.
        var farmTile = FindObjectOfType<Farm>();
        if (farmTile != null && farmTile.isWatered) 
        {
            Debug.Log("물 주기 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForSeededSoil()
    {
        // Farm 스크립트의 isOccupied 상태(씨앗이 심겨진 상태)를 확인합니다.
        var farmTile = FindObjectOfType<Farm>();
        if (farmTile != null && farmTile.isOccupied)
        {
            Debug.Log("씨앗 심기 확인!");
            return true;
        }
        return false;
    }

    private bool CheckBenchInteraction()
    {
        // 현재 활성화된 씬의 이름이 작업대 씬(예: "bench")인지 확인합니다.
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "bench")
        {
            Debug.Log("작업대 씬 이동 확인!");
            return true;
        }
        return false;
    }

    #endregion
}