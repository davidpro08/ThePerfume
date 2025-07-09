using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    // 인벤토리 일단 리스트로 구현했는데, 배열이 더 나을까요?
    // 슬롯에 아이템을 넣어야 해서 이게 적당하지는 않을 것 같은데
    public List<ItemSlot> items = new List<ItemSlot>();

    // 용량
    // 기본 용량 = 8
    // 나중에 추가 용량을 얻으면 capacity를 늘리기
    public int capacity = 8;

    // TODO: 로직이 이렇게 간단하면 안되고, 나중에 수정해야 함
    // 1. 같은 아이템이 있으면 어떻게 처리할것인가?
    // 2. 아이템이 앞부터 쌓이는 걸 어떻게 알 것인가?
}