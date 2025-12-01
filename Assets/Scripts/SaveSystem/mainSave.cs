using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class mainSave : MonoBehaviour
{
    public static mainSave Instance { get; private set; }

    [Header("UI")]
    public Button play;
    public Button load;

    private const string FileName = "ThePerfumeSaveFile.json";
    private static string FilePath => System.IO.Path.Combine(Application.persistentDataPath, FileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        CheckSaveFile();

        if (play != null)
        {
            play.onClick.AddListener(OnPlayClicked);
        }
        if (load != null)
        {
            load.onClick.AddListener(OnLoadClicked);
        }
    }

    // 저장 파일 존재 여부 확인 및 load 활성화 여부
    public void CheckSaveFile()
    {
        bool SaveFileExists = File.Exists(FilePath);
        if (load != null)
        {
            load.interactable = SaveFileExists;

            // 비활성화 보여주기 (아직 안 만듬)
        }
    }

    // Play 버튼 클릭 시
    public void OnPlayClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetGame();
            Player.ResetPosition();
        }
    }

    //laod 버튼 클릭 시
    public void OnLoadClicked()
    {
        // SaveManager에서 불러오기 처리
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }
}
