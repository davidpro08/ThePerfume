
// 튜토리얼 단계의 완료 조건을 정의하는 열거형입니다.
public enum TutorialConditionType
{
    None, // 특별한 조건 없이 바로 다음으로 넘어가는 경우
    CheckForTilledSoil,   // 흙이 설치되었는지 확인
    CheckForWateredSoil,  // 흙에 물을 주었는지 확인
    CheckForSeededSoil,   // 흙에 씨앗을 심었는지 확인
    InteractedWithIsolde // 이졸데와 상호작용했는지 확인
}
