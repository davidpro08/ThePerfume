using System;
using System.Collections.Generic;
using UnityEngine;

public class CSVDialogueParser : MonoBehaviour
{
    [Header("CSV 설정")]
    [Tooltip("인스펙터에서 대화 CSV 파일을 여기에 할당하세요.")]
    public TextAsset dialogueCSV;

    [Header("데이터 검증")]
    [Tooltip("CSV에 반드시 있어야 하는 컬럼들입니다.")]
    public string[] requiredColumns = { "ID", "NPC_ID", "DIALOGUE_TEXT" };

    private DialogueData dialogueData;

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
        LoadDialogueData();
    }

    void LoadDialogueData()
    {
        if (dialogueCSV == null)
        {
            Debug.LogError("Dialogue CSV 파일이 할당되지 않았습니다!");
            return;
        }

        try
        {
            // 새로운 CSVParser의 object 타입 파싱 메서드 사용 (대소문자 구분 없음)
            var parsedData = CSVParser.ParseFromTextAsObject(dialogueCSV.text, true);

            // 데이터 검증 (대소문자 구분 없음)
            if (!ValidateDialogueData(parsedData))
            {
                Debug.LogError("대화 CSV 데이터 검증에 실패했습니다.");
                return;
            }

            dialogueData = InterpretDialogueData(parsedData);
            Debug.Log($"총 {dialogueData.dialogues.Count}개의 대화 데이터를 로드했습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"대화 데이터 로드 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 대화 CSV 데이터의 유효성을 검증합니다.
    /// </summary>
    private bool ValidateDialogueData(List<Dictionary<string, object>> parsedData)
    {
        if (parsedData == null || parsedData.Count == 0)
        {
            Debug.LogWarning("파싱된 대화 데이터가 없습니다.");
            return false;
        }

        // 필수 컬럼 검증 (대소문자 구분 없음)
        var firstRow = parsedData[0];
        foreach (var column in requiredColumns)
        {
            if (!CSVParser.ContainsKeyCaseInsensitive(firstRow, column))
            {
                Debug.LogError($"필수 컬럼 '{column}'이(가) 대화 CSV에 없습니다.");
                return false;
            }
        }

        // 데이터 행 검증
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

            // Next_Dialogue_IDs가 비어있으면 건너뛰기
            string nextDialogueIdsStr = GetString(row, "NEXT_DIALOGUE_IDS");
            if (string.IsNullOrEmpty(nextDialogueIdsStr))
            {
                Debug.LogWarning($"대화 ID {id}: NEXT_DIALOGUE_IDS가 비어있습니다. 건너뜁니다.");
                continue;
            }

            // IS_END_DIALOGUE이 true이면 현재 NPC의 다음 start_ID를 next_Dialogue_ID로 설정
            bool isEndDialogue = GetBool(row, "IS_END_DIALOGUE");
            string[] nextDialogueIds;

            if (isEndDialogue)
            {
                // 대화 종료 시 현재 NPC의 다음 시작 대화 ID를 설정
                string npcId = GetString(row, "NPC_ID");
                var npcDialogues = data.GetDialoguesByNpcId(npcId);
                if (npcDialogues.Count > 0)
                {
                    // 첫 번째 대화를 다음 시작 대화로 설정
                    nextDialogueIds = new string[] { npcDialogues[0].id };
                }
                else
                {
                    // NPC 대화가 없으면 빈 배열로 설정
                    nextDialogueIds = new string[0];
                }
            }
            else
            {
                // 일반적인 경우 파이프(|)로 구분된 다음 대화 ID들 파싱
                nextDialogueIds = nextDialogueIdsStr.Split('|');
            }

            // Condition을 NpcState Enum으로 파싱 (빈칸이면 Default)
            NpcState condition = NpcStateUtility.ParseState(GetString(row, "CONDITION"));

            var entry = new DialogueEntry(
                id,
                GetString(row, "NPC_ID"),
                GetString(row, "DIALOGUE_TEXT").Replace("\\n", "\n"), // Allow newline characters in dialogue
                GetString(row, "CHOICES").Split('|'),
                nextDialogueIds,
                condition,
                isEndDialogue
            );
            data.dialogues.Add(entry);
        }

        return data;
    }

    public DialogueData GetDialogueData()
    {
        return dialogueData;
    }

    public DialogueEntry GetDialogueById(string id)
    {
        return dialogueData?.GetDialogueById(id);
    }

    public List<DialogueEntry> GetDialoguesByNpcId(string npcId)
    {
        return dialogueData?.GetDialoguesByNpcId(npcId) ?? new List<DialogueEntry>();
    }

    #region Helper Methods

    private string GetString(Dictionary<string, object> dict, string key)
    {
        // 대소문자 구분 없이 값을 가져오기
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