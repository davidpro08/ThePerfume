using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("튜토리얼 단계 설정")]
    [Tooltip("이 튜토리얼에서 진행할 모든 단계를 ScriptableObject 애셋으로 여기에 등록하세요.")]
    public List<TutorialStepSO> tutorialSteps;

    [Header("필수 연결")]
    public Npc guide; // 가이드 NPC (인스펙터에서 할당)

    private TutorialStepSO currentStep; // 현재 진행 중인 튜토리얼 단계
    private HashSet<TutorialStepSO> completedMakingSteps = new HashSet<TutorialStepSO>(); // 완료된 단계들을 저장
    private HashSet<TutorialStepSO> completedFlowerSteps = new HashSet<TutorialStepSO>(); // 완료된 단계들을 저장
    private string _lastEndedDialogueId = "tutorial_001_001"; // 마지막으로 종료된 대화 ID를 캐시
    private bool hasInteractedWithIsolde = false; // 이졸데와 상호작용했는지 여부

    private const string FINAL_ID = "alric_022";
    private const string START_ID = "tutorial_001_001";

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

        Debug.Log($"스토리 시작 여부: {SaveManager.Instance.CurrentSave.story.isPrologueCompleted}");
        if (!SaveManager.Instance.CurrentSave.story.isPrologueCompleted)
        {
            Debug.Log("프롤로그 스토리 진행 중이므로 스토리씬으로 이동합니다.");
            SceneManager.sceneLoaded += OnStorySceneLoaded;
            SceneManager.LoadScene("StoryScene");
        }
        else
        {
            StartTutorialLogic();
        }
    }

    private void OnStorySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StoryScene")
        {
            SceneManager.sceneLoaded -= OnStorySceneLoaded;

            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.PlayStorySequence(StoryManager.Instance.IntroCsvFile, "Intro_dialogue", StartTutorialLogic);
            }
            else
            {
                Debug.LogError("StoryManager 인스턴스를 찾을 수 없습니다.");
                StartTutorialLogic();
            }
        }
    }

    private void StartTutorialLogic()
    {
        StartCoroutine(InitializeTutorial());
    }

    private IEnumerator InitializeTutorial()
    {
        yield return new WaitForSeconds(1f); // Wait for other managers to be ready

        var tutorialData = SaveManager.Instance.CurrentSave?.tutorial;

        // Case 1: Tutorial is already completed.
        if (tutorialData != null && tutorialData.isTutorialEnd)
        {
            Debug.Log("튜토리얼이 이미 완료되었습니다.");
            gameObject.SetActive(false);
            yield break; // Stop the coroutine
        }

        // Restore the set of completed steps from save data
        completedMakingSteps.Clear();
        if (tutorialData?.completedStepNames != null)
        {
            foreach (var stepName in tutorialData.completedStepNames)
            {
                var step = tutorialSteps.FirstOrDefault(s => s.name == stepName);
                if (step != null) completedMakingSteps.Add(step);
            }
        }

        // 마지막으로 끝난 대화 ID를 복원
        _lastEndedDialogueId = tutorialData?.currentStep ?? "";

        // Case 2: Resume tutorial from a saved point.
        if (!string.IsNullOrEmpty(_lastEndedDialogueId))
        {
            Debug.Log($"저장된 데이터로부터 튜토리얼을 재개합니다. 마지막 대화 ID: {_lastEndedDialogueId}");
            HandleDialogueEnd(null, _lastEndedDialogueId);
        }
        // Case 3: Start tutorial from the very beginning.
        else
        {
            Debug.Log("저장된 튜토리얼 데이터가 없거나, 시작 지점 정보가 없어 새로 시작합니다.");
            NpcDialogueManager.Instance.StartDialogue(guide, "Tutorial_makingPerfume", START_ID);
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

    // NpcDialogueManager에서 대화가 끝났을 때 호출될 핸들러
    private void HandleDialogueEnd(Npc npc, string dialogueId)
    {
        if (npc != null && npc.GetNpcId() == "Isolde")
        {
            hasInteractedWithIsolde = true;
        }

        _lastEndedDialogueId = dialogueId; // 항상 마지막 대화 ID를 캐시

        var stepToStart = tutorialSteps.FirstOrDefault(step => step.triggerId == dialogueId && !completedMakingSteps.Contains(step));

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

    // 현재 단계를 완료 처리
    private void CompleteStep(TutorialStepSO step)
    {
        Debug.Log($"튜토리얼 단계 완료: {step.name}");

        completedMakingSteps.Add(step);
        currentStep = null;

        // 다음 대화 시작
        if (!string.IsNullOrEmpty(step.nextDialogueId))
        {
            NpcDialogueManager.Instance.StartDialogue(guide, "Tutorial_makingPerfume", step.nextDialogueId);
        }

        CheckAllMakingTutorialsCompleted();
    }

    private void CheckAllMakingTutorialsCompleted()
    {

        bool isAllFinished = tutorialSteps.All(step => completedMakingSteps.Contains(step));
        if (isAllFinished)
        {
            Debug.Log("모든 향수 제작 튜토리얼 단계가 완료되었습니다!");
            SaveManager.Instance.CurrentSave.tutorial.isTutorialEnd = true;
            SaveManager.Instance.SaveGame();

            StoryManager.Instance.PlayStorySequence(StoryManager.Instance.nextStroyCsvFile, "Chapter1_dialogue", () => Debug.Log("다음 챕터 시작!"));
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
            case TutorialConditionType.InteractedWithIsolde:
                return CheckInteractedWithIsolde();
            case TutorialConditionType.CheckForClickedBench:
                return CheckForClickedBench();
            case TutorialConditionType.CheckForSelectedRose:
                return CheckForSelectedRose();
            case TutorialConditionType.CheckForClickedRose:
                return CheckForClickedRose();
            case TutorialConditionType.CheckForNativeRose:
                return CheckForNativeRose();
            case TutorialConditionType.CheckForHandledAllRose:
                return CheckForHandledAllRose();
            case TutorialConditionType.CheckForClickedExit:
                return CheckForClickedExit();
            case TutorialConditionType.CheckforClickedTill:
                return CheckforClickedTill();
            case TutorialConditionType.ChecnkForSelectedRoseLeaf:
                return ChecnkForSelectedRoseLeaf();
            case TutorialConditionType.CheckForClickedRoseTube:
                return CheckForClickedRoseTube();
            case TutorialConditionType.CheckForSelectedFuel:
                return CheckForSelectedFuel();
            case TutorialConditionType.CheckForClickedFuelTube:
                return CheckForClickedFuelTube();
            case TutorialConditionType.CheckForClickedMixture:
                return CheckForClickedMixture();
            case TutorialConditionType.CheckForClickedBaseTube:
                return CheckForClickedBaseTube();
            case TutorialConditionType.CheckForClickedMiiddleTube:
                return CheckForClickedMiiddleTube();
            case TutorialConditionType.CheckForTopTube:
                return CheckForTopTube();
            case TutorialConditionType.CheckForPutLiqiuid:
                return CheckForPutLiqiuid();
            case TutorialConditionType.CheckForMixedPerfume:
                return CheckForMixedPerfume();
            case TutorialConditionType.CheckForClickedPerfumeAndClickedExit:
                return CheckForClickedPerfumeAndClickedExit();
            case TutorialConditionType.CheckForClickedBowl:
                return CheckForClickedBowl();
            case TutorialConditionType.None:
                return true; // 'None' 조건은 항상 참
            default:
                return false;
        }
    }

    #region === 세이브 / 로드 관련 메소드 ===

    /// <summary>
    /// SaveManager가 호출할 메서드. 전달받은 GameSave 객체에 현재 튜토리얼 진행 상황을 기록합니다.
    /// </summary>
    public void PrepareSaveData(GameSave save)
    {
        if (save.tutorial == null)
        {
            save.tutorial = new TutorialSaveData();
        }

        // 마지막으로 끝난 대화 ID를 저장하여, 언제나 정확한 재개 지점을 확보.
        save.tutorial.currentStep = _lastEndedDialogueId;

        // 완료된 스텝들의 이름을 저장.
        save.tutorial.completedStepNames = new HashSet<string>(completedMakingSteps.Select(s => s.name));

        // 모든 튜토리얼 단계가 완료되었는지 확인
        if (tutorialSteps.All(step => completedMakingSteps.Contains(step)))
        {
            save.tutorial.isTutorialEnd = true;
        }
    }

    public void ResetTutorial()
    {
        completedMakingSteps.Clear();
        currentStep = null;
        _lastEndedDialogueId = START_ID;
        hasInteractedWithIsolde = false;

        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(InitializeTutorial());
    }

    #endregion

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

    private bool CheckInteractedWithIsolde()
    {
        if (hasInteractedWithIsolde)
        {
            Debug.Log("이졸데와 상호작용 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedBench()
    {
        if (SceneManager.GetActiveScene().name == "bench")
        {
            Debug.Log("벤치 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForSelectedRose()
    {
        if (InventoryManager.Instance.EquippedItem().name == "Rose")
        {
            Debug.Log("장미 선택 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedRose()
    {
        if (BenchUIManager.Instance.HasSpawnedItemOnTray())
        {
            Debug.Log("장미 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForNativeRose()
    {
        if (!FlowerManager.Instance.IsExitable())
        {
            Debug.Log("원예 장미 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForHandledAllRose()
    {
        if (!BenchUIManager.Instance.HasSpawnedItemOnTray())
        {
            Debug.Log("모든 장미 처리 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedBowl()
    {
        if (FlowerManager.Instance.IsExitable())
        {
            Debug.Log("꽃그릇 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedExit()
    {
        if (SceneManager.GetActiveScene().name == "lab")
        {
            Debug.Log("나가기 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckforClickedTill()
    {
        if (SceneManager.GetActiveScene().name == "distiller")
        {
            Debug.Log("증류기 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool ChecnkForSelectedRoseLeaf()
    {
        if (InventoryManager.Instance.EquippedItem().name == "RosePetal")
        {
            Debug.Log("장미잎 선택 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedRoseTube()
    {
        Distiller distiller = FindAnyObjectByType<Distiller>();

        if (distiller != null && distiller.FindFirstPetalMaterial() != null)
        {
            Debug.Log("장미관 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForSelectedFuel()
    {
        if (InventoryManager.Instance.EquippedItem().name == "Fuel")
        {
            Debug.Log("연료 선택 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedFuelTube()
    {
        Distiller distiller = FindAnyObjectByType<Distiller>();

        if (distiller != null && distiller.HasAtLeastOneFuel())
        {
            Debug.Log("연료 선택 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedMixture()
    {
        if (SceneManager.GetActiveScene().name == "Mixture")
        {
            Debug.Log("조합대 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedBaseTube()
    {
        Mixture mixture = FindAnyObjectByType<Mixture>();

        if (mixture != null && mixture.baseData != null)
        {
            Debug.Log("베이스관 클릭 확인!");
            return true;
        }

        return false;
    }

    private bool CheckForClickedMiiddleTube()
    {
        Mixture mixture = FindAnyObjectByType<Mixture>();

        if (mixture != null && mixture.middleData != null)
        {
            Debug.Log("미들관 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForTopTube()
    {
        Mixture mixture = FindAnyObjectByType<Mixture>();

        if (mixture != null && mixture.topData != null)
        {
            Debug.Log("탑관 클릭 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForPutLiqiuid()
    {
        Mixture mixture = FindAnyObjectByType<Mixture>();

        if (mixture != null && mixture.pBaseData != null && mixture.pMiddleData != null && mixture.pTopData != null)
        {
            Debug.Log("액체 넣기 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForMixedPerfume()
    {
        Mixture mixture = FindAnyObjectByType<Mixture>();

        if (mixture != null && mixture.perfumeData != null)
        {
            Debug.Log("향수 혼합 확인!");
            return true;
        }
        return false;
    }

    private bool CheckForClickedPerfumeAndClickedExit()
    {
        for (int i = InventoryManager.Instance.itemSlots.Count - 1; i >= 0; i--)
        {
            ItemSlot slot = InventoryManager.Instance.itemSlots[i];
            if (slot.itemData.itemType == ItemType.Perfume && SceneManager.GetActiveScene().name == "lab")
            {
                Debug.Log("향수 클릭 및 나가기 확인!");
                return true;
            }
        }
        return false;
    }

    #endregion
}