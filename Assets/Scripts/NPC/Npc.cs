using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class Npc : MonoBehaviour, IInteract
{
    [Header("NPC 설정")]
    [SerializeField] protected string npcId = "npc_001";
    [SerializeField] protected string startDialogueId = "";

    [Header("초상화 설정")]
    [SerializeField] protected NpcPortraitData portraitData;

    [Header("기존 호환성")]
    [SerializeField] List<String> dialogueObjects = new();

    [Header("움직임 세팅")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float moveDuration = 0.8f;
    [SerializeField] private float nextMoveTime = 2f;
    [SerializeField] private Vector2 minArea = new Vector2(0f, 0f);
    [SerializeField] private Vector2 maxArea = new Vector2(5f, 5f);

    [Header("애니메이션 세팅")]
    [SerializeField] protected Animator _animator;

    private NpcState currentState = NpcState.Default;

    protected virtual void Start()
    {
        InitializePortrait();

        StartCoroutine(RandomMove(minArea, maxArea));
    }

    protected void InitializePortrait()
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

        NpcDialogueManager.Instance.StartDialogue(this, "Daily", startDialogueId);
    }

    public bool CanInteract(Player player)
    {

        if (CSVDialogueParser.Instance == null)
        {
            Debug.LogError("CSVDialogueParser가 없습니다!");
            return false;
        }

        var npcDialogues = CSVDialogueParser.Instance.GetDialoguesByNpcId("Daily", npcId);

        if (npcDialogues.Count == 0)
        {
            Debug.Log($"NPC {npcId}의 대화 데이터가 없습니다!");
            return false;
        }
        return true;
    }

    IEnumerator RandomMove(Vector2 minBounds, Vector2 maxBounds)
    {
        //초기 움직임
        Vector2 startMoveVector = Vector2.zero;
        Vector2 targetPos = (Vector2)transform.position;
        float initialMoving = 0f;

        int x = Random.Range(-1, 2);
        int y = Random.Range(-1, 2);

        if (x != 0 || y != 0)
        {
            startMoveVector = new Vector2(x, y).normalized;
            targetPos = (Vector2)transform.position + startMoveVector * moveSpeed;

            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

            Vector2 actualMoveVector = targetPos - (Vector2)transform.position;
            if (actualMoveVector.magnitude > 0.01f)
            {
                UpdateAnimator(startMoveVector);
                transform.DOMove(targetPos, moveDuration);
                initialMoving = moveDuration;
            }
        }

        if (initialMoving > 0f)
        {
            yield return new WaitForSeconds(initialMoving);
            UpdateAnimator(Vector2.zero);
        }

        // 반복 움직임
        while (true)
        {
            if (NpcDialogueManager.Instance.isActive)
            {
                UpdateAnimator(Vector2.zero);
                transform.DOKill();
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(nextMoveTime);

            x = Random.Range(-1, 2);
            y = Random.Range(-1, 2);

            Vector2 moveVector = new Vector2(x, y);//.normalized;

            if (moveVector.sqrMagnitude < 0.01f)
            {
                UpdateAnimator(Vector2.zero);
                continue;
            }

            moveVector = moveVector.normalized;
            targetPos = (Vector2)transform.position + moveVector * moveSpeed;

            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

            Vector2 actualMoveVector = targetPos - (Vector2)transform.position;
            if (actualMoveVector.magnitude > 0.01f)
            {
                UpdateAnimator(moveVector);
                transform.DOMove(targetPos, moveDuration);
            }
            //UpdateAnimator(moveVector);

            //transform.DOMove((Vector2)transform.position + moveVector * moveSpeed, nextMoveTime);
        }
    }

    protected virtual void UpdateAnimator(Vector2 input)
    {
        float magnitude = input.magnitude;

        if (magnitude <= 0.01f)
        {
            // Idle 상태에서는 isWalking만 false, 방향 값은 그대로 유지
            _animator.SetBool("isWalking", false);
            return;
        }

        // 움직이는 경우에만 파라미터 갱신
        _animator.SetBool("isWalking", true);

        _animator.SetFloat("InputX", input.x);
        _animator.SetFloat("InputY", input.y);

        _animator.SetFloat("LastInputX", input.x);
        _animator.SetFloat("LastInputY", input.y);
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