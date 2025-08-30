
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Tutorial Step", order = 0)]
public class TutorialStepSO : ScriptableObject
{
    [Header("트리거 설정")]
    [Tooltip("이 튜토리얼 단계를 활성화시키는 대화 ID입니다. 이 대화가 끝나면 조건 확인을 시작합니다.")]
    public string triggerId;

    [Header("완료 조건")]
    [Tooltip("이 단계가 완료되기 위해 만족해야 하는 조건입니다.")]
    public TutorialConditionType conditionType;

    [Header("완료 후 설정")]
    [Tooltip("조건을 만족했을 때 다음에 출력될 대화 ID입니다.")]
    public string nextDialogueId;
}
