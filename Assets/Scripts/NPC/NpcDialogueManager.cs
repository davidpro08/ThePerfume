using TMPro;
using UnityEngine;

    public class NpcDialogueManager : MonoBehaviour
    {
        [Header("대화창 관련 요소")]
        public GameObject dialogueObject;
        public TextMeshProUGUI dialogueText;
        
        // 싱글톤 인스턴스
        public static NpcDialogueManager Instance { get; private set; }

        void Awake()
        {
            //이미 인스턴스 있으면 자신 파괴
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        

        /// <summary>
        /// 대화창 설정
        /// </summary>
        /// <returns></returns>
        public void SetDialogueText(string text)
        {
            if (dialogueObject != null)
            {
                Debug.Log($"[{name}] : dialogueObject 요소 연결 필요");
            }
            
            if(dialogueText != null)
            {
                Debug.Log($"[{name}] : dialogueText 요소 연결 필요");
            }

            if (dialogueObject != null) dialogueObject.SetActive(dialogueObject.activeSelf);
            if (dialogueText != null) dialogueText.text = text;
        }
    }