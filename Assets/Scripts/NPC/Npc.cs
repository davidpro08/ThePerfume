using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Npc : MonoBehaviour, IInteract
{
    [Header("NPC 설정")]
    [SerializeField] string npcId = "npc_001";
    [SerializeField] string startDialogueId = ""; // 비어있으면 첫 번째 대화 사용

    [Header("초상화 설정")]
    [SerializeField] private NpcPortraitData portraitData;

    [Header("기존 호환성")]
    [SerializeField] List<String> dialogueObjects = new();

    private NpcState currentState = NpcState.Default;

    void Start()
    {
        // 초상화 데이터가 설정되지 않았다면 자동으로 찾기
        if (portraitData == null)
        {
            // Resources 폴더에서 NPC ID로 초상화 데이터 찾기
            portraitData = Resources.Load<NpcPortraitData>($"NPC/PortraitData/{npcId}");
            if (portraitData == null)
            {
                Debug.LogWarning($"NPC {npcId}의 초상화 데이터를 찾을 수 없습니다. Resources/NPC/PortraitData/{npcId} 경로를 확인해주세요.");
            }
        }
    }



    public void Interact(Player player)
    {
        if (!CanInteract(player)) return;

        NpcDialogueManager.Instance.StartDialogue(this, startDialogueId);

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
    /// NPC 상태 변경 시 초상화 업데이트 (나중에 NPC 스프라이트 변경용)
    /// </summary>
    /// <param name="newState">새로운 NPC 상태</param>
    public void UpdatePortrait(NpcState newState)
    {
        if (portraitData == null)
        {
            Debug.LogWarning($"NPC {npcId}의 초상화 데이터가 설정되지 않았습니다.");
            return;
        }

        currentState = newState;
        // 나중에 NPC 스프라이트 변경 시 사용할 예정
        Debug.Log($"NPC {npcId} 상태를 {newState}로 변경했습니다.");
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

    /// <summary>
    /// NPC ID 반환
    /// </summary>
    /// <returns>NPC ID</returns>
    public string GetNpcId()
    {
        return npcId;
    }

    /// <summary>
    /// 시작 대화 ID 반환
    /// </summary>
    /// <returns>시작 대화 ID</returns>
    public string GetStartDialogueId()
    {
        return startDialogueId;
    }

    /// <summary>
    /// 현재 NPC 상태 반환
    /// </summary>
    /// <returns>현재 NPC 상태</returns>
    public NpcState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// 초상화 데이터 설정
    /// </summary>
    /// <param name="data">초상화 데이터</param>
    public void SetPortraitData(NpcPortraitData data)
    {
        portraitData = data;
        // 설정 후 현재 상태로 초상화 업데이트
        UpdatePortrait(currentState);
    }



    /// <summary>
    /// 현재 초상화 스프라이트 반환
    /// </summary>
    /// <returns>현재 초상화 스프라이트</returns>
    public Sprite GetCurrentPortraitSprite()
    {
        if (portraitData != null)
        {
            return portraitData.GetPortrait(currentState);
        }
        return null;
    }
}