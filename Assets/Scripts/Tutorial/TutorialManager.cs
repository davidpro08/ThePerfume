using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("튜토리얼 단계 설정")]
    [Tooltip("이 튜토리얼에서 진행할 모든 단계를 ScriptableObject 애셋으로 여기에 등록하세요.")]
    public List<TutorialStepSO> tutorialSteps;
    
    [Header("필수 연결")]
    public Npc guide; // 가이드 NPC (인스펙터에서 할당)

    private TutorialStepSO currentStep; // 현재 진행 중인 튜토리얼 단계
    private HashSet<TutorialStepSO> completedSteps = new HashSet<TutorialStepSO>(); // 완료된 단계들을 저장

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
        }
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
        if (currentStep != null)
        {
            // 현재 단계의 완료 조건을 확인
            if (CheckCondition(currentStep.conditionType))
            {
                CompleteStep(currentStep);
            }
        }
    }

    // 튜토리얼 시작
    private System.Collections.IEnumerator StartTutorialSequence()
    {
        yield return new WaitForSeconds(1f); // 게임 시작 후 잠시 대기
        NpcDialogueManager.Instance.StartDialogue(guide, "Tutorial", "narration_001_001");
    }

    // NpcDialogueManager에서 대화가 끝났을 때 호출될 핸들러
    private void HandleDialogueEnd(string dialogueId)
    {
        var stepToStart = tutorialSteps.FirstOrDefault(step => step.triggerId == dialogueId && !completedSteps.Contains(step));

        if (stepToStart != null)
        {
            Debug.Log($"튜토리얼 단계 시작: {stepToStart.name} (트리거 ID: {dialogueId})");

            // 현재 단계로 설정
            currentStep = stepToStart;
            
            // 조건 시작 전 플레이어에게 아이템 제공
            foreach (GiveItem giveItem in currentStep.giveItems)
            {
                InventoryManager.Instance.AddItem(giveItem.itemData, giveItem.amount);
            }
            
            // 만약 조건이 'None'이라면 바로 완료 처리
            if (currentStep.conditionType == TutorialConditionType.None)
            {
                CompleteStep(currentStep);
            }
        }
    }

    // 조건 시작 전 플레이어에게 아이템 제공
    public void GiveItem(ItemData itemData, int amountOfItems)
    {
        InventoryManager.Instance.AddItem(itemData, 1);
    }

    // 현재 단계를 완료 처리
    private void CompleteStep(TutorialStepSO step)
    {
        Debug.Log($"튜토리얼 단계 완료: {step.name}");

        completedSteps.Add(step);
        currentStep = null;

        // 다음 대화 시작
        if (!string.IsNullOrEmpty(step.nextDialogueId))
        {
            NpcDialogueManager.Instance.StartDialogue(guide, "Tutorial", step.nextDialogueId);
        }
    }

    // 완료 조건 타입에 따라 적절한 확인 메소드를 호출
    private bool CheckCondition(TutorialConditionType conditionType)
    {
        switch (conditionType)
        {
            case TutorialConditionType.CheckForTilledSoil:
                return CheckForTilledSoil();
            case TutorialConditionType.CheckForWateredSoil:
                return CheckForWateredSoil();
            case TutorialConditionType.CheckForSeededSoil:
                return CheckForSeededSoil();
            case TutorialConditionType.CheckBenchInteraction:
                return CheckBenchInteraction();
            case TutorialConditionType.None:
                return true; // 'None' 조건은 항상 참
            default:
                return false;
        }
    }

    #region === 조건 확인 메소드들 ===

    private bool CheckForTilledSoil()
    {
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
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "bench")
        {
            Debug.Log("작업대 씬 이동 확인!");
            return true;
        }
        return false;
    }

    #endregion
}