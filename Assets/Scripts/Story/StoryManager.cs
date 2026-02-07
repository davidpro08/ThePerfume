using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using static UnityEngine.TextAsset;

[System.Serializable]
public class StoryEvent
{
    public string type; //MOVE, LOOK, DIALOGUE, EFFECT, SOUND, WAIT 등
    public string charID;  // 캐릭터 아이디
    public Vector2 startPos;
    public Vector2 endPos;
    public Vector2 direction;
    public float duration;
    public string value;    // 대사ID, 효과이름 등
    public string background;   //배경 이미지 이름
    public string sfx;           //효과음 이름
}

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;
    [SerializeField] List<CharacterMotion> characters;
    [SerializeField] GameObject characterParent;
    List<StoryEvent> storyEvents = new List<StoryEvent>(); // 추후 CSV에서 파생해온 데이터
    [Header("Story CSV File")]
    [SerializeField] public UnityEngine.TextAsset IntroCsvFile;
    [SerializeField] public UnityEngine.TextAsset nextStroyCsvFile;
    [SerializeField] public UnityEngine.TextAsset dialogueFile;
    private string dialogueID = "";
    private int currentDialogueIndex = 0;
    [Header("Background Settings")]
    [SerializeField] Transform backgroundParent;
    [Header("Status")]
    public bool isStoryMode = false;
    public bool isPrologueDone = false;
    public bool isChapter1Done = false;

    public Dictionary<string, GameObject> backgroundDict = new Dictionary<string, GameObject>();
    private const float BG_FADE_DURATION = 1.0f;
    private System.Action onStoryCompleteCallBackF;

    private float currentLightingAlpha = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {

        if (backgroundParent != null)
        {
            foreach (Transform child in backgroundParent)
            {
                backgroundDict[child.name] = child.gameObject;
                child.gameObject.SetActive(false);
            }
        }

        if (characterParent != null)
        {
            DontDestroyOnLoad(characterParent);
        }
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.CloseHotbar();
        }

        LoadStoryData();

        CheckAndResumeStory();
    }

    public void PlayStorySequence(UnityEngine.TextAsset csvFile, string newDialogueID, System.Action onComplete)
    {
        if (csvFile == null) return;

        if (!string.IsNullOrEmpty(newDialogueID))
        {
            this.dialogueID = newDialogueID;
        }

        SetStoryMode(true);

        onStoryCompleteCallBackF = onComplete;
        LoadStory(csvFile);
        StartCoroutine(PlayStory());
    }

    public void LoadStory(UnityEngine.TextAsset csvFile)
    {
        var parsedData = CSVParser.ParseFromTextAsObject(csvFile.text, true);
        storyEvents.Clear();

        foreach (var row in parsedData)
        {
            StoryEvent evt = new StoryEvent();
            evt.type = row["type"].ToString();

            if (row.ContainsKey("charID")) evt.charID = row["charID"].ToString();
            else evt.charID = "";

            evt.startPos = new Vector2(ParseFloat(row, "startX"), ParseFloat(row, "startY"));
            evt.endPos = new Vector2(ParseFloat(row, "endX"), ParseFloat(row, "endY"));
            evt.direction = new Vector2(ParseFloat(row, "dirX"), ParseFloat(row, "dirY"));
            evt.duration = ParseFloat(row, "duration", 0f);
            if (row.ContainsKey("value")) evt.value = row["value"].ToString();
            if (row.ContainsKey("background")) evt.background = row["background"].ToString();
            evt.sfx = row["sfx"].ToString();

            storyEvents.Add(evt);
        }
    }

    IEnumerator ShowDialogue(int count)
    {
        if (dialogueFile == null)
        {
            Debug.LogError("대화 CSV 파일이 할당되지 않았습니다.");
            yield break;
        }

        var data = CSVDialogueParser.Instance.GetDialogueData(dialogueID);

        if (data == null)
        {
            Debug.LogError($"대화 데이터를 찾을 수 없습니다: {dialogueID}");
            yield break;
        }
        if (data == null || data.dialogues == null)
        {
            Debug.LogError($"대화 데이터가 비어있습니다: {dialogueID}");
            yield break;
        }

        for (int i = 0; i < count; i++)
        {
            if (currentDialogueIndex < data.dialogues.Count)
            {
                var entry = data.dialogues[currentDialogueIndex];

                yield return StartCoroutine(NpcDialogueManager.Instance.StartStoryDialogue(entry));
                currentDialogueIndex++;
            }
            else
            {
                Debug.LogWarning("대사 데이터가 부족합니다.");
                break;
            }
        }

    }

    IEnumerator CameraShake(float duration)
    {
        Camera.main.transform.DOShakePosition(duration, 0.5f);
        yield return new WaitForSeconds(duration);
    }

    IEnumerator ChangeBackground(string bgName, float targetAlpha = 0f)
    {
        LightingEffect.instance.SetLighting(1f, BG_FADE_DURATION);
        yield return new WaitForSeconds(BG_FADE_DURATION);

        ResetBackground();

        if (backgroundDict.ContainsKey(bgName))
        {
            GameObject newBG = backgroundDict[bgName];
            newBG.SetActive(true);

            if (CameraManager.instance != null)
            {
                CameraManager.instance.SetOverviewTarget(newBG);
            }
        }
        else
        {
            Debug.LogWarning($"배경 이미지 '{bgName}'을(를) 찾을 수 없습니다.");
        }
        yield return new WaitForSeconds(0.1f);
        LightingEffect.instance.SetLighting(targetAlpha, BG_FADE_DURATION);
        yield return new WaitForSeconds(BG_FADE_DURATION);
    }

    // CSV를 파싱할 때 charID,type(eventType), startPos, endPos, direction, duration, background
    // 대사는 별도 파싱

    IEnumerator PlayStory()
    {
        foreach (var evt in storyEvents)
        {
            if (evt.type == "EFFECT")
            {
                if (evt.value == "FADE_TO_70") currentLightingAlpha = 0.7f;
                else if (evt.value == "FADE_IN") currentLightingAlpha = 0f;
                else if (evt.value == "FADE_OUT") currentLightingAlpha = 1f;
            }

            // 배경 우선 변경
            if (!string.IsNullOrEmpty(evt.background))
            {
                yield return StartCoroutine(ChangeBackground(evt.background, currentLightingAlpha));
            }

            CharacterMotion targetChar = GetCharacter(evt.charID);

            switch (evt.type)
            {
                case "ON":
                    if (targetChar != null)
                        targetChar.gameObject.GetComponent<SpriteRenderer>().enabled = true;
                    else
                        Debug.LogWarning($"캐릭터 '{evt.charID}'을(를) 찾을 수 없습니다.");
                    break;
                case "OFF":
                    if (targetChar != null)
                        targetChar.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    else
                        Debug.LogWarning($"캐릭터 '{evt.charID}'을(를) 찾을 수 없습니다.");
                    break;
                case "TELEPORT":
                    if (targetChar != null)
                        targetChar.SetStartPosition(GetPosition(evt.endPos));
                    else
                        Debug.LogWarning($"캐릭터 '{evt.charID}'을(를) 찾을 수 없습니다.");
                    break;
                case "MOVE":
                    targetChar.CharacterWalk(GetPosition(evt.startPos), GetPosition(evt.endPos), evt.duration);
                    yield return new WaitForSeconds(evt.duration + 0.2f);
                    break;
                case "SLIDE":
                    targetChar.CharacterWalk(GetPosition(evt.startPos), GetPosition(evt.endPos), evt.duration, false);
                    yield return new WaitForSeconds(evt.duration + 0.2f);
                    break;
                case "ANIMATION":
                    if (targetChar != null)
                        targetChar.PlayAnimation(evt.value);
                    else
                        Debug.LogWarning($"캐릭터 '{evt.charID}'을(를) 찾을 수 없습니다.");
                    break;
                // 씬 이동
                case "SCENE_CHANGE":
                    ResetBackground();
                    UnityEngine.SceneManagement.SceneManager.LoadScene(evt.value);
                    yield return new WaitForSeconds(1.0f); // 씬 전환 대기
                    SetStoryMode(true);
                    break;
                case "LOOK":
                    targetChar.CharacterLookAt(evt.direction);
                    break;

                case "DIALOGUE":
                    yield return StartCoroutine(ShowDialogue(int.TryParse(evt.value, out int count) ? count : 1));
                    break;
                case "EFFECT":
                    if (evt.value == "SHAKE") yield return StartCoroutine(CameraShake(evt.duration));
                    else if (evt.value == "FADE_TO_70")
                    {
                        LightingEffect.instance.SetLighting(0.7f, evt.duration);
                        yield return new WaitForSeconds(evt.duration);
                    }
                    else if (evt.value == "FADE_IN")
                    {
                        LightingEffect.instance.SetLighting(0f, evt.duration);
                        yield return new WaitForSeconds(evt.duration);
                    }
                    else if (evt.value == "FADE_OUT")
                    {
                        LightingEffect.instance.SetLighting(1f, evt.duration);
                        yield return new WaitForSeconds(evt.duration);
                    }
                    else if (evt.value == "LETTERBOX_TRUE")
                    {
                        LetterboxEffect.instance.SetLetterbox(true);
                        yield return new WaitForSeconds(1.0f);
                    }
                    else if (evt.value == "LETTERBOX_FALSE")
                    {
                        LetterboxEffect.instance.SetLetterbox(false);
                        yield return new WaitForSeconds(1.0f);
                    }
                    else if (evt.value == "VIGNETTE_TRUE")
                    {
                        VolumeEffect.instance.SetVignette(true);
                        yield return new WaitForSeconds(1.0f);
                    }
                    else if (evt.value == "VIGNETTE_FALSE")
                    {
                        VolumeEffect.instance.SetVignette(false);
                        yield return new WaitForSeconds(1.0f);
                    }
                    break;

                case "SFX": // 단발성
                    SoundManager.Instance.PlaySFX(evt.sfx);
                    break;

                case "SFX_START": // 수동제어
                    SoundManager.Instance.PlayLoopSFX(evt.sfx, float.Parse(evt.value));
                    break;

                case "SFX_STOP":
                    SoundManager.Instance.StopLoopSFX(evt.sfx);
                    break;

                case "WAIT":
                    yield return new WaitForSeconds(evt.duration);
                    break;

                case "ITEM":
                    ItemData itemData = ItemDataBase.Instance.GetItemByID(int.Parse(evt.value));
                    NoticeUIManager.Instance.ShowNoticeCanvas($"{itemData.itemName} 획득!");
                    InventoryManager.Instance.AddItem(itemData, 1);
                    yield return new WaitForSeconds(0.5f);
                    break;

                case "BGM_START":
                    BGMType bgmType = SoundManager.Instance.FindByName(evt.value);
                    SoundManager.Instance.PlayBGM(bgmType);
                    break;

                case "BGM_STOP":
                    SoundManager.Instance.StopBGM();
                    break;
            }

            yield return new WaitForSeconds(0.05f);
        }

        ResetBackground();

        isPrologueDone = true;
        saveStoryData();

        SetStoryMode(false);

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.OpenHotbar();
        }

        onStoryCompleteCallBackF?.Invoke();
        onStoryCompleteCallBackF = null;
    }

    #region Helpers
    // 캐릭터 아이디에 맞춰서 캐릭터 배열 반환하는 함수 필요함.
    public CharacterMotion GetCharacter(string id)
    {
        foreach (var character in characters)
        {
            if (character.name == id)
            {
                return character;
            }
        }
        return null;
    }

    private void onPrologueComplete()
    {
        isPrologueDone = true;
        saveStoryData();
    }

    private void onChapter1Complete()
    {
        isChapter1Done = true;
        saveStoryData();

        UnityEngine.SceneManagement.SceneManager.LoadScene("lab");
    }

    private void ToggleEffects(bool isActive)
    {
        if (LightingEffect.instance != null)
        {
            Debug.Log("ToggleEffect 인스턴스 찾음");
            LightingEffect.instance.gameObject.SetActive(isActive);
            if (!isActive) LightingEffect.instance.SetLighting(0f, 0f);
        }
        else
        {
            Debug.Log("ToggleEffect 인스턴스 못 찾음");
        }

        if (LetterboxEffect.instance != null)
        {
            Debug.Log("ToggleEffect 인스턴스 찾음");
            LetterboxEffect.instance.gameObject.SetActive(isActive);
            if (!isActive) LetterboxEffect.instance.SetLetterbox(false);
        }
        else
        {
            Debug.Log("ToggleEffect 인스턴스 못 찾음");
        }

        if (VolumeEffect.instance != null)
        {
            Debug.Log("ToggleEffect 인스턴스 찾음");
            VolumeEffect.instance.gameObject.SetActive(isActive);
            if (!isActive) VolumeEffect.instance.SetVignette(false);
        }
        else
        {
            Debug.Log("ToggleEffect 인스턴스 못 찾음");
        }
    }

    private void SetStoryMode(bool isStoryMode)
    {
        this.isStoryMode = isStoryMode;

        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            player.SetPlayerDisenabled(isStoryMode);
        }

        if (characterParent != null)
        {
            characterParent.SetActive(isStoryMode);
        }

        ToggleEffects(isStoryMode);

        if (isStoryMode)
        {
            CharacterMotion jang = GetCharacter("Jang");
            if (jang != null)
            {
                jang.gameObject.SetActive(true);
            }
        }

        saveStoryData();
    }

    public void ResetBackground()
    {
        if (backgroundDict == null) return;

        foreach (var bg in backgroundDict.Values)
        {
            if (bg != null) bg.SetActive(false);
        }
    }

    Vector2 GetPosition(Vector2 csvPos)
    {
        if (Mathf.Abs(csvPos.x) > 1.5f || Mathf.Abs(csvPos.y) > 1.5f)
        {
            Debug.LogWarning("CSV 위치 값이 비정상적입니다. 기본 위치(0,0)로 설정합니다.");
            return Vector2.zero;
        }

        Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(csvPos.x, csvPos.y, 10f));
        return new Vector2(worldPos.x, worldPos.y);
    }

    int ParseInt(Dictionary<string, object> row, string key, int defaultValue = 0)
    {
        if (row.ContainsKey(key) && int.TryParse(row[key].ToString(), out int result))
        {
            return result;
        }
        return defaultValue;
    }

    float ParseFloat(Dictionary<string, object> row, string key, float defaultValue = 0f)
    {
        if (row.ContainsKey(key) && float.TryParse(row[key].ToString(), out float result))
        {
            return result;
        }
        return defaultValue;
    }
    #endregion

    #region save/load
    public void saveStoryData()
    {
        var storyData = SaveManager.Instance.CurrentSave.story;

        storyData.isPrologueCompleted = isPrologueDone;
        storyData.lastDialougeIndex = currentDialogueIndex;
        storyData.isChapter1Done = isChapter1Done;
        storyData.isStoryMode = isStoryMode;

        SaveManager.Save(SaveManager.Instance.CurrentSave);
    }

    public void LoadStoryData()
    {
        var storyData = SaveManager.Instance.CurrentSave.story;
        isPrologueDone = storyData.isPrologueCompleted;
        currentDialogueIndex = storyData.lastDialougeIndex;
        isChapter1Done = storyData.isChapter1Done;
        isStoryMode = storyData.isStoryMode;

        if (this.isStoryMode) SetStoryMode(true);
    }

    public void CheckAndResumeStory()
    {
        if (!isPrologueDone)
        {
            PlayStorySequence(IntroCsvFile, "Intro_dialogue", onPrologueComplete);
        }
        else if (!SaveManager.Instance.CurrentSave.tutorial.isTutorialEnd)
        {

            SetStoryMode(false);
        }
        else if (!isChapter1Done)
        {
            PlayStorySequence(nextStroyCsvFile, "Chapter1_dialogue", onChapter1Complete);
        }
        else
        {
            SetStoryMode(false);
        }
    }
    #endregion
}