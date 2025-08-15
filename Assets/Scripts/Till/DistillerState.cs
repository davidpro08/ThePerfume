using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DistillerState
{
    public string distillerID;
    public List<ItemData> currentIngredient = new List<ItemData>(); // 존재하는 재료
    public float currentProgress = 0f; // 증류 진행도
    public EssenceData completedProduct = null; // 완성된 아이템
}
