using Unity.VisualScripting;
using UnityEngine;

public class TillSceneManager : MonoBehaviour
{
    private string loadedDistillerID;
    private DistillerState currentDistillerState;

    [Header("UI 요소")]
    public GameObject tubeFuelSpawnPoint1;
    public GameObject tubeFuelSpawnPoint2;
    public GameObject tubeFuelSpawnPoint3;
    public GameObject tubePetalSpawnPoint;
    public GameObject tubeEssenceSpawnPoint;

    private GameObject spawnedFuelPrefab;
    private GameObject spawnedPetalPrefab;
    private GameObject spawnedEssencePrefab;

    void Start()
    {
        if (SceneChanger.Instance == null) Debug.Log("[TillSceneManager] SceneChanger.Instance == null");

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
            currentDistillerState = new DistillerState();
            UpdateTillUI();
        }

        SceneChanger.Instance.ResetDistillerID();
    }

    void UpdateTillUI()
    {
        if (currentDistillerState == null) return;

        // 재료들 업데이트
        if (TillUIManager.Instance != null)
        {
            TillUIManager.Instance.ClearAllDisplayedTubeItem();
        }
        else
        {
            Debug.Log("[TillSceneManager] TillUIManager.Instance == null");
            return;
        }

        int fuelCount = 0;
        for (int i = 0; i < currentDistillerState.currentIngredient.Count; i++)
        {
            ItemData ingredients = currentDistillerState.currentIngredient[i];
            MaterialData ingredient = ingredients as MaterialData;
            if (ingredient == null || ingredient.itemPrefab == null) continue;

            if (ingredient.itemName == "Fuel")
            {
                if (fuelCount == 0 && TillUIManager.Instance.tubeFuelTransformPos.Count > 0)
                {
                    TillUIManager.Instance.DisplayItemOnTube(ingredient, TillUIManager.Instance.tubeFuelTransformPos[0]);
                }
                else if (fuelCount == 1 && TillUIManager.Instance.tubeFuelTransformPos.Count > 1)
                {
                    TillUIManager.Instance.DisplayItemOnTube(ingredient, TillUIManager.Instance.tubeFuelTransformPos[1]);
                }
                else if (fuelCount == 2 && TillUIManager.Instance.tubeFuelTransformPos.Count > 2)
                {
                    TillUIManager.Instance.DisplayItemOnTube(ingredient, TillUIManager.Instance.tubeFuelTransformPos[1]);
                    fuelCount++;
                }
                else
                {
                    if (TillUIManager.Instance.tubePetalTransformPos != null)
                    {
                        TillUIManager.Instance.DisplayItemOnTube(ingredient, TillUIManager.Instance.tubePetalTransformPos[0]);
                    }
                }
            }
        }

        if (currentDistillerState.completedProduct != null && currentDistillerState.completedProduct.prefabInTube != null)
        {
            if (TillUIManager.Instance.tubeEssenceTransformPos != null)
            {
                TillUIManager.Instance.DisplayItemOnTube(currentDistillerState.completedProduct, TillUIManager.Instance.tubeEssenceTransformPos);
            }
        }

        // currentDistillerStat 변경할 때 아래 코드 호출 필요함
        // TillDataManager.Instance.UpdateDistillerState(loadedDistillerID, currentDistillerState);
    }
}
