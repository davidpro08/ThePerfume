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
    public string condition;
    public bool isEndDialogue;

    public DialogueEntry(string id, string npcId, string dialogueText, string[] choices = null, string[] nextDialogueIds = null, string condition = "", bool isEndDialogue = false)
    {
        this.id = id;
        this.npcId = npcId;
        this.dialogueText = dialogueText;
        this.choices = choices ?? new string[0];
        this.nextDialogueIds = nextDialogueIds ?? new string[0];
        this.condition = condition;
        this.isEndDialogue = isEndDialogue;
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
}