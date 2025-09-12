using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Npc : MonoBehaviour, IInteract
{
    [Header("NPC 설정")]
    [SerializeField] string npcId = "npc_001";
    [SerializeField] string startDialogueId = ""; // 비어있으면 첫 번째 대화 사용

    [Header("초상화 설정")]
    [SerializeField] private NpcPortraitData portraitData;

    [Header("기존 호환성")]
    [SerializeField] List<String> dialogueObjects = new();

    [Header("움직임 세팅")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float nextMoveTime = 2f;
    
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

        StartCoroutine(RandomMove());
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player)) return;

        NpcDialogueManager.Instance.StartDialogue(this, startDialogueId);
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
    }

    IEnumerator RandomMove()
    {
        while (true)
        {
            yield return new WaitForSeconds(nextMoveTime);

            int x = Random.Range(-1, 2);
            int y = Random.Range(-1, 2);

            Vector3 direction = new Vector3(x, y, 0).normalized;
            transform.position += direction * moveSpeed;
        }
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
    /// 현재 초상화 스프라이트 반환
    /// </summary>
    /// <returns>현재 초상화 스프라이트</returns>
    public Sprite GetCurrentPortraitSprite(NpcState npcState)
    {
        if (portraitData != null)
        {
            return portraitData.GetPortrait(npcState);
        }
        return null;
    }
}