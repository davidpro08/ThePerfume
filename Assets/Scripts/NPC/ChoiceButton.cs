using System;
using TMPro;
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

        public void OtherSelected()
        {
            gameObject.SetActive(false);
        }
    }
}