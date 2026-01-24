using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] List<CharacterMotion> characters;
    [SerializeField] GameObject characterParent;
    List<StoryEvent> storyEvents = new List<StoryEvent>(); // 추후 CSV에서 파생해온 데이터
    [Header("Story CSV File")]
    [SerializeField] TextAsset storyCsvFile;
    [SerializeField] TextAsset dialogueFile;
    private int currentDialogueIndex = 0;
    [Header("Background Settings")]
    [SerializeField] Transform backgroundParent;

    private Dictionary<string, GameObject> backgroundDict = new Dictionary<string, GameObject>();
    private const float BG_FADE_DURATION = 1.0f;
    public bool isPrologueDone = false;

    void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null) LoadStoryData();
        if (isPrologueDone)
        {
            if (characterParent != null) characterParent.SetActive(false);
            return;
        }

        if (backgroundParent != null)
        {
            foreach (Transform child in backgroundParent)
            {
                backgroundDict[child.name] = child.gameObject;
                child.gameObject.SetActive(false);
            }
        }
        if (storyCsvFile != null)
        {
            LoadStory(storyCsvFile);
            StartCoroutine(PlayStory());
        }
        else
        {
            Debug.LogError("스토리 CSV 파일이 할당되지 않았습니다!");
        }

        if (characterParent != null)
        {
            DontDestroyOnLoad(characterParent);
        }
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.CloseHotbar();
        }
    }

    public void LoadStory(TextAsset csvFile)
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
        // 대사 파일명 안 정해짐
        var data = CSVDialogueParser.Instance.GetDialogueData(dialogueFile.name);
        if (data == null || data.dialogues == null) yield break;

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

    IEnumerator ChangeBackground(string bgName)
    {
        LightingEffect.instance.SetLighting(1f, BG_FADE_DURATION);
        yield return new WaitForSeconds(BG_FADE_DURATION);

        if (backgroundDict.ContainsKey(bgName))
        {
            foreach (var bg in backgroundDict.Values)
            {
                bg.SetActive(false);
            }
            backgroundDict[bgName].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"배경 이미지 '{bgName}'을(를) 찾을 수 없습니다.");
        }
        yield return new WaitForSeconds(0.1f);
        LightingEffect.instance.SetLighting(0f, BG_FADE_DURATION);
        yield return new WaitForSeconds(BG_FADE_DURATION);
    }

    // CSV를 파싱할 때 charID,type(eventType), startPos, endPos, direction, duration, background
    // 대사는 별도 파싱

    IEnumerator PlayStory()
    {
        if (characterParent != null)
        {
            characterParent.SetActive(true);
        }

        foreach (var evt in storyEvents)
        {
            // 배경 우선 변경
            if (!string.IsNullOrEmpty(evt.background))
            {
                yield return StartCoroutine(ChangeBackground(evt.background));
            }

            CharacterMotion targetChar = GetCharacter(evt.charID);
            switch (evt.type)
            {
                case "MOVE":
                    targetChar.CharacterWalk(evt.startPos, evt.endPos, evt.duration);
                    yield return new WaitForSeconds(evt.duration + 0.2f);
                    break;
                case "SLIDE":
                    targetChar.CharacterWalk(evt.startPos, evt.endPos, evt.duration, false);
                    yield return new WaitForSeconds(evt.duration + 0.2f);
                    break;
                case "ANIMATION":
                    targetChar.PlayAnimation(evt.value);
                    break;
                // 씬 이동
                case "SCENE_CHANGE":
                    UnityEngine.SceneManagement.SceneManager.LoadScene(evt.value);
                    yield return new WaitForSeconds(1.0f); // 씬 전환 대기
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
            }

            yield return new WaitForSeconds(0.05f);
        }
        isPrologueDone = true;
        saveStoryData();

        if (characterParent != null)
        {
            characterParent.SetActive(false);
        }
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.OpenHotbar();
        }
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
        SaveManager.Save(SaveManager.Instance.CurrentSave);
    }

    public void LoadStoryData()
    {
        var storyData = SaveManager.Instance.CurrentSave.story;
        isPrologueDone = storyData.isPrologueCompleted;
        currentDialogueIndex = storyData.lastDialougeIndex;
    }
    #endregion
}