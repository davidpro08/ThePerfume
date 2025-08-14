using Unity.VisualScripting;
using UnityEngine;

public class TillSceneManager : MonoBehaviour
{
    private string loadedDistillerID;
    private DistillerState currentDistillerState;

    void Start()
    {
        loadedDistillerID = SceneChanger.Instance.currentDistillerID;

        if (!string.IsNullOrEmpty(loadedDistillerID))
        {
            currentDistillerState = TillDataManager.Instance.GetDistillerState(loadedDistillerID);
            if (currentDistillerState != null)
            {
                Debug.Log($"Till [증류기 {loadedDistillerID}] 씬 입장~");
                UpdateTillUI();
            }
            else
            {
                Debug.Log($"Till [증류기 {loadedDistillerID}] 씬 오류...");
            }
        }
        else
        {
            Debug.Log("Till 씬 입장~ ID 없음...");
        }

        SceneChanger.Instance.ResetDistillerID();
    }

    void UpdateTillUI()
    {
        if (currentDistillerState == null) return;

        // 재료들 업데이트
        // currentDistillerStat 변경할 때 아래 코드 호출 필요함
        // TillDataManager.Instance.UpdateDistillerState(loadedDistillerID, currentDistillerState);
    }
}
