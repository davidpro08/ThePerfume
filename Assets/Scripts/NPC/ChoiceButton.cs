using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace NPC
{
    public class ChoiceButton : MonoBehaviour
    {
        public Button button;
        public TextMeshProUGUI choiceText;
        public String currentDialogue;
        public String nextDialogueId;

        
        private void OnEnable()
        {
            choiceText.text = currentDialogue;
        }

        public void Setting(string text, string nextId)
        {
            currentDialogue = text;
            nextDialogueId = nextId;
            gameObject.SetActive(true);
        }

        public void ThisSelected()
        {
            Debug.Log($"현재 선택된 선택지 : {name}");
        }

        public void OtherSelected()
        {
            gameObject.SetActive(false);
        }
    }
}