
    using System.Collections;
    using UnityEngine;

    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;
        public Npc guide;
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // TODO: 나중에 실제 튜토리얼 했는지 파악하는거로 바꿔야함
            if (true)
            {
                Debug.Log("튜토리얼 시작");
                
                StartCoroutine(StartTutorial());
            }
        }

        IEnumerator StartTutorial()
        {
            yield return new WaitForSeconds(1f);
            NpcDialogueManager.Instance.StartDialogue(guide);
        }
    }