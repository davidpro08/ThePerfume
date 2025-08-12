using System;
using UnityEngine;
using System.Collections.Generic;

// 모든 증류기의 데이터 관리 >> 
// 새로운 스크립트로 프리팹에 tiller.cs 붙여서 고유ID로 각 상태 확인해야하나

public class TillDataManager : MonoBehaviour
{
    public static TillDataManager Instance { get; private set; }
    public int EntryMethodToTill { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
