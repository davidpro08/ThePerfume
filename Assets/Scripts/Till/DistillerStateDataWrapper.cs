using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class DistillerStateDataWrapper
{
    // 딕셔너리로 저장하려니깐 문제가 생겨버려서..
    public List<DistillerState> states = new List<DistillerState>();
    public DistillerStateDataWrapper() { }
    public DistillerStateDataWrapper(Dictionary<string, DistillerState> dict)
    {
        foreach (var pair in dict)
        {
            states.Add(pair.Value);
        }
    }

    public Dictionary<string, DistillerState> toDictionary()
    {
        Dictionary<string, DistillerState> dict = new Dictionary<string, DistillerState>();
        foreach (var state in states)
        {
            if (!string.IsNullOrEmpty(state.distillerID))
            {
                dict[state.distillerID] = state;
            }
        }
        return dict;
    }
}
