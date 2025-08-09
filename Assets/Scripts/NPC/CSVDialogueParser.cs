using System;
using System.Collections.Generic;
using UnityEngine;

public class CSVDialogueParser : MonoBehaviour
{
    [Header("CSV 설정")]
    [Tooltip("인스펙터에서 대화 CSV 파일을 여기에 할당하세요.")]
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
            Debug.LogError("Dialogue CSV 파일이 할당되지 않았습니다!");
            return;
        }

        var parsedData = CSVParser.ParseFromText(dialogueCSV.text);
        dialogueData = InterpretDialogueData(parsedData);
        Debug.Log($"총 {dialogueData.dialogues.Count}개의 대화 데이터를 로드했습니다.");
    }

    private DialogueData InterpretDialogueData(List<Dictionary<string, object>> parsedData)
    {
        var data = new DialogueData();

        foreach (var row in parsedData)
        {
            string id = GetString(row, "id");
            if (string.IsNullOrEmpty(id)) continue;

            var entry = new DialogueEntry(
                id,
                GetString(row, "npcId"),
                GetString(row, "dialogueText").Replace("\\n", "\n"), // Allow newline characters in dialogue
                GetString(row, "choices").Split('|'),
                GetString(row, "nextDialogueIds").Split('|'),
                GetString(row, "condition"),
                GetBool(row, "isEndDialogue")
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
        return dict.ContainsKey(key) ? dict[key].ToString() : "";
    }

    private bool GetBool(Dictionary<string, object> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            string val = dict[key].ToString();
            return val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    #endregion
}