using System;
using System.Collections.Generic;
using UnityEngine;

public class CSVDialogueParser : MonoBehaviour
{
    [Header("CSV 설정")]
    public TextAsset dialogueCSV;

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
            Debug.LogError("Dialogue CSV 파일이 설정되지 않았습니다!");
            return;
        }

        dialogueData = ParseCSV(dialogueCSV.text);
        Debug.Log($"총 {dialogueData.dialogues.Count}개의 대화 데이터를 로드했습니다.");
    }

    DialogueData ParseCSV(string csvText)
    {
        DialogueData data = new DialogueData();
        string[] lines = csvText.Split('\n');

        // 첫 번째 줄은 헤더이므로 건너뛰기
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = ParseCSVLine(line);
            if (values.Length >= 3)
            {
                string id = values[0];
                string npcId = values[1];
                string dialogueText = values[2];

                // 선택지 처리 (4번째 컬럼)
                string[] choices = new string[0];
                if (values.Length > 3 && !string.IsNullOrEmpty(values[3]))
                {
                    choices = values[3].Split('|');
                }

                // 다음 대화 ID 처리 (5번째 컬럼)
                string[] nextDialogueIds = new string[0];
                if (values.Length > 4 && !string.IsNullOrEmpty(values[4]))
                {
                    nextDialogueIds = values[4].Split('|');
                }

                // 조건 처리 (6번째 컬럼)
                string condition = values.Length > 5 ? values[5] : "";

                // 종료 대화 여부 (7번째 컬럼)
                bool isEndDialogue = values.Length > 6 && values[6].ToLower() == "true";

                DialogueEntry entry = new DialogueEntry(id, npcId, dialogueText, choices, nextDialogueIds, condition, isEndDialogue);
                data.dialogues.Add(entry);
            }
        }

        return data;
    }

    string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }

        result.Add(currentValue);
        return result.ToArray();
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
}