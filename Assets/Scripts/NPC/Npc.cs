using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Npc : MonoBehaviour, IInteract
{
    [Header("NPC 설정")]
    [SerializeField] string npcId = "npc_001";
    [SerializeField] string startDialogueId = ""; // 비어있으면 첫 번째 대화 사용

    [Header("기존 호환성")]
    [SerializeField] List<String> dialogueObjects = new();

    public void Interact(Player player)
    {
        if (!CanInteract(player)) return;

        NpcDialogueManager.Instance.StartDialogue(npcId, startDialogueId);

        // // 기존 호환성 유지
        // if (dialogueObjects.Count > 0)
        // {
        //     NpcDialogueManager.Instance.SetDialogueText(dialogueObjects[0]);
        // }
    }

    public bool CanInteract(Player player)
    {

        if (CSVDialogueParser.Instance == null)
        {
            Debug.LogError("CSVDialogueParser가 없습니다!");
            return false;
        }

        var npcDialogues = CSVDialogueParser.Instance.GetDialoguesByNpcId(npcId);

        if (npcDialogues.Count == 0)
        {
            Debug.Log($"NPC {npcId}의 대화 데이터가 없습니다!");
            return false;
        }
        return true;

        // // 기존 호환성 유지
        // if (dialogueObjects.Count == 0)
        // {
        //     Debug.Log("대사가 하나도 없음");
        //     return false;
        // }
        // return true;
    }

    /// <summary>
    /// NPC ID 설정
    /// </summary>
    public void SetNpcId(string newNpcId)
    {
        npcId = newNpcId;
    }

    /// <summary>
    /// 시작 대화 ID 설정
    /// </summary>
    public void SetStartDialogueId(string dialogueId)
    {
        startDialogueId = dialogueId;
    }
}