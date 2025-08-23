using System;
using UnityEngine;

/// <summary>
/// NPC의 상태를 정의하는 열거형
/// </summary>
public enum NpcState
{
    Default,        // 기본 상태
    Happy,          // 행복한 상태
    Sad,            // 슬픈 상태
    Angry,          // 화난 상태
    Surprised,      // 놀란 상태
    Confused,       // 혼란스러운 상태
    Excited,        // 흥분한 상태
    Calm,           // 차분한 상태
    Nervous,        // 긴장한 상태
    Friendly,       // 친근한 상태
    Hostile         // 적대적인 상태
}

/// <summary>
/// NPC 상태 관련 유틸리티 클래스
/// </summary>
public static class NpcStateUtility
{
    /// <summary>
    /// 문자열을 NpcState로 변환합니다.
    /// </summary>
    /// <param name="stateString">상태 문자열</param>
    /// <returns>NpcState 열거형 값</returns>
    public static NpcState ParseState(string stateString)
    {
        if (string.IsNullOrEmpty(stateString))
            return NpcState.Default;

        // 대소문자 구분 없이 파싱
        if (Enum.TryParse<NpcState>(stateString, true, out NpcState result))
            return result;

        // 파싱 실패 시 기본값 반환
        Debug.LogWarning($"알 수 없는 NPC 상태: {stateString}. 기본 상태로 설정합니다.");
        return NpcState.Default;
    }
    
}
