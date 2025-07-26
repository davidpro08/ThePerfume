using TMPro;
using UnityEngine;

    public class NpcDialogueManager : MonoBehaviour
    {
        [Header("대화창 관련 요소")]
        public GameObject dialogueObject;
        public TextMeshProUGUI dialogueText;
        public bool isActive = false;
        
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
            if (dialogueObject == null)
            {
                Debug.Log($"[{name}] : dialogueObject 요소 연결 필요");
                return;
            }
            
            if(dialogueText == null)
            {
                Debug.Log($"[{name}] : dialogueText 요소 연결 필요");
                return;
            }
            
            // 상태 반전
            isActive = !isActive;
            
            // 텍스트 설정하고 열거나 닫는 부분
            dialogueText.text = text;
            dialogueObject.SetActive(isActive);
            
            // TODO: 플레이어 통제를 위해 설정했는데, 안할거면 로직 바꾸기
            PauseManager.Instance.TogglePause();
        }
    }