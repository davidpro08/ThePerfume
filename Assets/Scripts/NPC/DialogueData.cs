using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public string id;
    public string npcId;
    public string dialogueText;
    public string[] choices;
    public string[] nextDialogueIds;
    public NpcState condition;  // Enum으로 변경
    public bool isEndDialogue;

    public DialogueEntry(string id, string npcId, string dialogueText, string[] choices = null, string[] nextDialogueIds = null, NpcState condition = NpcState.Default, bool isEndDialogue = false)
    {
        this.id = id;
        this.npcId = npcId;
        this.dialogueText = dialogueText;
        this.choices = choices ?? new string[0];
        this.nextDialogueIds = nextDialogueIds ?? new string[0];
        this.condition = condition;
        this.isEndDialogue = isEndDialogue;
    }

    /// <summary>
    /// 선택지가 표시되어야 하는지 확인합니다.
    /// Next_Dialogue_ID가 2개 이상일 때만 선택지를 표시합니다.
    /// </summary>
    /// <returns>선택지 표시 여부</returns>
    public bool ShouldShowChoices()
    {
        return nextDialogueIds != null && nextDialogueIds.Length >= 2;
    }

    /// <summary>
    /// 다음 대화 ID가 유효한지 확인합니다.
    /// </summary>
    /// <returns>유효성 여부</returns>
    public bool HasValidNextDialogue()
    {
        return nextDialogueIds != null && nextDialogueIds.Length > 0 && 
               !string.IsNullOrEmpty(nextDialogueIds[0]);
    }

    /// <summary>
    /// 대화 종료 시 다음 시작 대화 ID를 반환합니다.
    /// </summary>
    /// <returns>다음 시작 대화 ID</returns>
    public string GetNextStartDialogueId()
    {
        if (nextDialogueIds != null && nextDialogueIds.Length > 0)
        {
            return nextDialogueIds[0];
        }
        return "";
    }
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();

    public DialogueEntry GetDialogueById(string id)
    {
        return dialogues.Find(d => d.id == id);
    }

    public List<DialogueEntry> GetDialoguesByNpcId(string npcId)
    {
        return dialogues.FindAll(d => d.npcId == npcId);
    }

    /// <summary>
    /// NPC의 시작 대화를 찾습니다.
    /// </summary>
    /// <param name="npcId">NPC ID</param>
    /// <returns>시작 대화 엔트리</returns>
    public DialogueEntry GetStartDialogue(string npcId)
    {
        var npcDialogues = GetDialoguesByNpcId(npcId);
        if (npcDialogues.Count > 0)
        {
            // 첫 번째 대화를 시작 대화로 사용
            return npcDialogues[0];
        }
        return null;
    }
}