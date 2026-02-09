using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class CharacterMotion : Npc
{
    // 사용 가능 : SetStartDialogueId(string dialogueId),
    // GetNpcId(), GetCurrentPortraitSprite(NpcState npcState)
    private Animator animator;

    protected override void Start()
    {
        InitializePortrait();
    }

    protected void Awake()
    {
        InitializePortrait();
        animator = GetComponent<Animator>();
    }

    // 캐릭터 시작 위치
    public void SetStartPosition(UnityEngine.Vector2 startPos)
    {
        transform.position = startPos;
    }

    // 캐릭터 이동
    public void CharacterWalk(UnityEngine.Vector2 startPos, UnityEngine.Vector2 endPos, float duration, bool useMoveAnim = true)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        StartCoroutine(WalkRoutine(startPos, endPos, duration, useMoveAnim));
    }

    // 캐릭터 원하는 방향으로 바라보기
    public void CharacterLookAt(UnityEngine.Vector2 direction)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        UpdateAnimator(direction, false);
    }

    // 애니메이션 출력
    public void PlayAnimation(string animationName)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        animator.Play(animationName);
    }

    private IEnumerator WalkRoutine(UnityEngine.Vector2 startPos, UnityEngine.Vector2 endPos, float duration, bool useMoveAnim)
    {
        float elapsed = 0f;
        transform.position = startPos;
        UnityEngine.Vector2 direction = (endPos - startPos).normalized;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = UnityEngine.Vector2.Lerp(startPos, endPos, elapsed / duration);

            if (useMoveAnim)
                UpdateAnimator(direction, true);
            else
                UpdateAnimator(direction, false);
            yield return null;
        }
        transform.position = endPos;
        UpdateAnimator(UnityEngine.Vector2.zero, false);
    }


    void UpdateAnimator(UnityEngine.Vector2 input, bool isWalking = true)
    {
        // 방향 갱신
        if (input.magnitude > 0.01f)
        {
            _animator.SetFloat("InputX", input.x);
            _animator.SetFloat("InputY", input.y);

            _animator.SetFloat("LastInputX", input.x);
            _animator.SetFloat("LastInputY", input.y);
        }

        // 걷는 애니메이션 재생 여부
        _animator.SetBool("isWalking", isWalking);
    }
}
