using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueFile
{
    public string dialogueName;
    public TextAsset dialogueCSV;
}

public class CSVDialogueParser : MonoBehaviour
{
    [Header("CSV 설정")]
    [Tooltip("인스펙터에서 대화 CSV 파일들을 여기에 할당하세요.")]
    public DialogueFile[] dialogueFiles;

    [Header("데이터 검증")]
    [Tooltip("CSV에 반드시 있어야 하는 컬럼들입니다.")]
    public string[] requiredColumns = { "ID", "NPC_ID", "DIALOGUE_TEXT" };

    private Dictionary<string, DialogueData> dialogueDataCollection = new Dictionary<string, DialogueData>();

    public static CSVDialogueParser Instance { get; private set; }

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
        LoadAllDialogueData();
    }

    void LoadAllDialogueData()
    {
        if (dialogueFiles == null || dialogueFiles.Length == 0)
        {
            Debug.LogError("Dialogue CSV 파일들이 할당되지 않았습니다!");
            return;
        }

        foreach (var dialogueFile in dialogueFiles)
        {
            if (dialogueFile.dialogueCSV == null)
            {
                Debug.LogError($"Dialogue CSV 파일이 할당되지 않았습니다: {dialogueFile.dialogueName}");
                continue;
            }

            try
            {
                var parsedData = CSVParser.ParseFromTextAsObject(dialogueFile.dialogueCSV.text, true);

                if (!ValidateDialogueData(parsedData))
                {
                    Debug.LogError($"대화 CSV 데이터 검증에 실패했습니다: {dialogueFile.dialogueName}");
                    continue;
                }

                DialogueData dialogueData = InterpretDialogueData(parsedData);
                dialogueDataCollection[dialogueFile.dialogueName] = dialogueData;
                Debug.Log($"총 {dialogueData.dialogues.Count}개의 대화 데이터를 로드했습니다: {dialogueFile.dialogueName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"대화 데이터 로드 중 오류 발생 ({dialogueFile.dialogueName}): {e.Message}");
            }
        }
    }

    private bool ValidateDialogueData(List<Dictionary<string, object>> parsedData)
    {
        if (parsedData == null || parsedData.Count == 0)
        {
            Debug.LogWarning("파싱된 대화 데이터가 없습니다.");
            return false;
        }

        var firstRow = parsedData[0];
        foreach (var column in requiredColumns)
        {
            if (!CSVParser.ContainsKeyCaseInsensitive(firstRow, column))
            {
                Debug.LogError($"필수 컬럼 '{column}'이(가) 대화 CSV에 없습니다.");
                return false;
            }
        }

        int validRows = 0;
        for (int i = 0; i < parsedData.Count; i++)
        {
            var row = parsedData[i];
            if (CSVParser.ContainsKeyCaseInsensitive(row, "ID") && !string.IsNullOrEmpty(GetString(row, "ID")))
            {
                validRows++;
            }
            else
            {
                Debug.LogWarning($"행 {i + 1}: 유효하지 않은 ID 또는 빈 ID");
            }
        }

        if (validRows == 0)
        {
            Debug.LogError("유효한 대화 데이터가 없습니다.");
            return false;
        }

        Debug.Log($"대화 데이터 검증 완료: {validRows}개 유효한 행");
        return true;
    }

    private DialogueData InterpretDialogueData(List<Dictionary<string, object>> parsedData)
    {
        var data = new DialogueData();

        foreach (var row in parsedData)
        {
            string id = GetString(row, "ID");
            if (string.IsNullOrEmpty(id)) continue;

            string nextDialogueIdsStr = GetString(row, "NEXT_DIALOGUE_IDS");
            if (string.IsNullOrEmpty(nextDialogueIdsStr))
            {
                Debug.LogWarning($"대화 ID {id}: NEXT_DIALOGUE_IDS가 비어있습니다. 건너뜁니다.");
                continue;
            }

            bool isEndDialogue = GetBool(row, "IS_END_DIALOGUE");
            string[] nextDialogueIds;

            if (isEndDialogue)
            {
                string npcId = GetString(row, "NPC_ID");
                var npcDialogues = data.GetDialoguesByNpcId(npcId);
                if (npcDialogues.Count > 0)
                {
                    nextDialogueIds = new string[] { npcDialogues[0].id };
                }
                else
                {
                    nextDialogueIds = new string[0];
                }
            }
            else
            {
                nextDialogueIds = nextDialogueIdsStr.Split('|');
            }

            NpcState condition = NpcStateUtility.ParseState(GetString(row, "CONDITION"));

            var entry = new DialogueEntry(
                id,
                GetString(row, "NPC_ID"),
                GetString(row, "DIALOGUE_TEXT").Replace("\\n", "\n"),
                GetString(row, "CHOICES").Split('|'),
                nextDialogueIds,
                condition,
                isEndDialogue
            );
            data.dialogues.Add(entry);
        }

        return data;
    }

    public DialogueData GetDialogueData(string dialogueName)
    {
        dialogueDataCollection.TryGetValue(dialogueName, out var dialogueData);
        return dialogueData;
    }

    public DialogueEntry GetDialogueById(string dialogueName, string id)
    {
        if (dialogueDataCollection.TryGetValue(dialogueName, out var dialogueData))
        {
            return dialogueData.GetDialogueById(id);
        }
        return null;
    }

    public List<DialogueEntry> GetDialoguesByNpcId(string dialogueName, string npcId)
    {
        if (dialogueDataCollection.TryGetValue(dialogueName, out var dialogueData))
        {
            return dialogueData.GetDialoguesByNpcId(npcId) ?? new List<DialogueEntry>();
        }
        return new List<DialogueEntry>();
    }

    public List<DialogueEntry> GetNonConditionalDialoguesByNpcId(string dialogueName, string npcId)
    {
        if (dialogueDataCollection.TryGetValue(dialogueName, out var dialogueData))
        {
            return dialogueData.GetNonConditionalDialoguesByNpcId(npcId) ?? new List<DialogueEntry>();
        }
        return new List<DialogueEntry>();
    }

    public void Parse(string dialogueName, string csvText)
    {
        if (string.IsNullOrEmpty(csvText))
        {
            Debug.Log($"[CSVDialogueParser] '{dialogueName}의 CSV 텍스트 비어있음");
            return;
        }

        try
        {
            var parsedData = CSVParser.ParseFromTextAsObject(csvText, true);

            if (!ValidateDialogueData(parsedData))
            {
                Debug.Log($"[CSVDialogueParser] '{dialogueName}' 데이터 검증 실패");
                return;
            }

            DialogueData dialogueData = InterpretDialogueData(parsedData);

            if (dialogueDataCollection.ContainsKey(dialogueName))
            {
                dialogueDataCollection[dialogueName] = dialogueData;
                Debug.Log($"[CSVDialogueParser] '{dialogueName}' 데이터 갱신");
            }
            else
            {
                dialogueDataCollection.Add(dialogueName, dialogueData);
                Debug.Log($"[CSVDialogueParser] '{dialogueName}' 데이터 새로 등록");
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[CSVDialogueParser] 오류 발생: {dialogueName}");
        }
    }

    #region Helper Methods

    private string GetString(Dictionary<string, object> dict, string key)
    {
        var value = CSVParser.GetValueCaseInsensitive(dict, key);
        return value?.ToString() ?? "";
    }

    private bool GetBool(Dictionary<string, object> dict, string key)
    {
        var value = CSVParser.GetValueCaseInsensitive(dict, key);
        if (value != null)
        {
            string val = value.ToString();
            return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    #endregion
}