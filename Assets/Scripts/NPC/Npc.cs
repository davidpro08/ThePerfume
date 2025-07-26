using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Npc : MonoBehaviour, IInteract
{
    // 대사들
    [SerializeField] List<String> dialogueObjects = new ();
    
    public void Interact(Player player)
    {
        // 임시로 맨 첫번째만 사용
        if (!CanInteract(player)) return;
        
        
        NpcDialogueManager.Instance.SetDialogueText(dialogueObjects[0]);
    }

    public bool CanInteract(Player player)
    {
        if (dialogueObjects.Count == 0)
        {
            Debug.Log("대사가 하나도 없음");
            return false;
        }
        return true;
    }
}