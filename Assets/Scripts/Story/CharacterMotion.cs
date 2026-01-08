using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotion : Npc
{
    // 사용 가능 : SetStartDialogueId(string dialogueId),
    // GetNpcId(), GetCurrentPortraitSprite(NpcState npcState)
    private Animator animator;

    protected override void Start()
    {
        InitializePortrait();
        animator = GetComponent<Animator>();
    }

    // 캐릭터 이동
    public void CharacterWalk(Vector2 startPos, Vector2 endPos, float duration)
    {
        StartCoroutine(WalkRoutine(startPos, endPos, duration));
    }

    // 캐릭터 원하는 방향으로 바라보기
    public void CharacterLookAt(Vector2 direction)
    {
        UpdateAnimator(direction, false);
    }

    private IEnumerator WalkRoutine(Vector2 startPos, Vector2 endPos, float duration)
    {
        float elapsed = 0f;
        transform.position = startPos;
        Vector2 direction = (endPos - startPos).normalized;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, endPos, elapsed / duration);

            UpdateAnimator(direction, true);
            yield return null;
        }
        transform.position = endPos;
        UpdateAnimator(Vector2.zero, false);
    }


    void UpdateAnimator(Vector2 input, bool isWalking = true)
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
