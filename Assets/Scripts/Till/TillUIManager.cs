using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.InputSystem;
using System.Linq;

public class TillUIManager : MonoBehaviour
{
    public static TillUIManager Instance { get; private set; }

    [Header("인벤토리 경고창UI")]
    [SerializeField] private GameObject warningCanvas;
    [SerializeField] private TextMeshProUGUI warningMessageText;
    [SerializeField] private Button warningOkButton;


    [Header("아이템 생성 설정")]
    [SerializeField] private Transform itemSpawnTubeFuel; // 아이템이 올라갈 Fuel Tube
    [SerializeField] public List<Transform> tubeFuelTransformPos; // Fuel Tube 자식 (위치 지정)
    [SerializeField] private Transform itemSpawnTubePetal;
    [SerializeField] public List<Transform> tubePetalTransformPos; // Fuel Tube 자식 (위치 지정)
    [SerializeField] public Transform tubeEssenceTransformPos;
    [SerializeField] private float makingTime = 15f;

    private List<GameObject> spawnedItemOnTube = new List<GameObject>();
    public bool isMakingEssence = false;
    private GameObject spawnedEssence;
    private EssenceData currentEssenceData;
    public bool isWarningCanvasOpen = false;

    public GameObject currentFuel1 = null;
    public GameObject currentFuel2 = null;
    public GameObject currentFuel3 = null;
    public GameObject currentPetal = null;
    public GameObject currentEssence = null;

    void Awake()
    {
        Debug.Log($"Awake 실행");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (warningCanvas != null) warningCanvas.SetActive(false);

        if (warningOkButton != null)
        {
            warningOkButton.onClick.RemoveAllListeners();
            warningOkButton.onClick.AddListener(OnWarningCanvasOkButton);
        }


    }

    void OnDestroy()
    {
        if (warningOkButton != null) warningOkButton.onClick.RemoveAllListeners();
    }

    // 경고창 표시
    public void ShowWarningCanvas(string message)
    {
        if (warningCanvas != null)
        {
            warningMessageText.text = message;
            warningCanvas.SetActive(true);
            isWarningCanvasOpen = true;
        }
    }
    // 경고창 끄기
    public void OnWarningCanvasOkButton()
    {
        if (warningCanvas != null)
        {
            warningCanvas.SetActive(false);
            isWarningCanvasOpen = false;
        }
    }

    // ============================ 시스템 관련 코드 =====================================
    // Tube에 아이템 랜덤 생성 > 투입
    public void SpawnItemOnTube(ItemData itemToSpawn, int count, List<Transform> transformPos)
    {
        Debug.Log($"SpawnItemOnTray. 아이템: {itemToSpawn?.name}, 수량: {count}");

        MaterialData materialData = itemToSpawn as MaterialData;
        if (itemToSpawn == null || materialData == null || materialData.itemPrefab == null || itemSpawnTubeFuel == null)
        {
            if (itemToSpawn == null) Debug.Log("itemToSpawn=null");
            if (materialData == null) Debug.Log($"materialData == null");
            if (materialData.itemPrefab == null) Debug.Log($"materialData.itemPrefab == null");
            if (itemSpawnTubeFuel == null) Debug.Log($"itemSpawnTubeFuel==null");
            return;
        }

        List<Transform> currnetSpawnPoint = null;

        if (itemToSpawn.itemName == "Fuel")
        {
            currnetSpawnPoint = tubeFuelTransformPos;
        }
        else if (itemToSpawn.itemType == ItemType.Material)
        {
            currnetSpawnPoint = new List<Transform>(tubePetalTransformPos);
        }

        if (currnetSpawnPoint == null || currnetSpawnPoint.Count == 0 || currnetSpawnPoint.Any(t => t == null))
        {
            Debug.Log("[SpawnItemOnTube] 스폰 포인트 못 팢음");
            return;
        }

        int spawnd = 0;

        foreach (Transform spawnPoint in transformPos)
        {
            if (spawnPoint == null)
            {
                Debug.Log("[SpawnItemOnTube] spawnPoint 중 오브젝트 파괴");
                continue;
            }

            if (spawnPoint.childCount > 0)
            {
                foreach (Transform child in spawnPoint)
                {
                    Destroy(child.gameObject);
                }
            }

            if (spawnPoint.childCount == 0)
            {
                GameObject spawndItem = Instantiate(materialData.itemPrefab, spawnPoint.position, Quaternion.identity);
                if (spawndItem == null)
                {
                    Debug.Log($"Instantiate 실패");
                    continue;
                }

                spawndItem.transform.SetParent(spawnPoint);
                spawnedItemOnTube.Add(spawndItem);

                Debug.Log($"{spawndItem.name} {spawnd + 1}개 생성");
                spawnd++;
                InventoryManager.Instance.RemoveItem(itemToSpawn, 1);

                DistillerState state = TillDataManager.Instance.GetDistillerState(SceneChanger.Instance.currentDistillerID);
                if (state != null && itemToSpawn != null)
                {
                    state.currentIngredient.Add(itemToSpawn);
                    TillDataManager.Instance.UpdateDistillerState(SceneChanger.Instance.currentDistillerID, state);
                }

                if (spawnd >= count) break;
            }
        }
        if (spawnd < count)
        {
            Debug.Log("트레이 빈칸 부족");
        }
        TryMakingEssence();
    }

    private void TryMakingEssence()
    {
        int fuelCount = 0;
        int petalCount = 0;
        MaterialData foundPetal = null;

        foreach (GameObject item in spawnedItemOnTube)
        {
            TubeItemDisplay display = item.GetComponent<TubeItemDisplay>();

            if (display == null) continue;

            ItemData data = display.myItemData;
            if (data.itemName == "Fuel") fuelCount++;
            else if (data is MaterialData material)
            {
                petalCount++;
                foundPetal = material;
            }
        }

        if (!isMakingEssence && fuelCount >= 1 && petalCount >= 1 && foundPetal != null)
        {
            currentEssenceData = foundPetal.essenceData;
            if (currentEssenceData != null && currentEssenceData.prefabInTube != null)
            {
                Debug.Log("Essence making start");
                StartCoroutine(MakeEssenceCoroutine());
            }
            else
            {
                Debug.Log("essenceData or prefabInTube == null");
            }
        }
    }

    private IEnumerator MakeEssenceCoroutine()
    {
        isMakingEssence = true;
        yield return new WaitForSeconds(makingTime);

        foreach (GameObject item in spawnedItemOnTube)
        {
            Destroy(item);
        }
        spawnedItemOnTube.Clear();

        spawnedEssence = Instantiate(currentEssenceData.prefabInTube);
        spawnedEssence.transform.SetParent(tubeEssenceTransformPos, false);
        spawnedEssence.transform.localPosition = Vector3.zero;

        Debug.Log("에센스 생성 완료");
    }

    public void OnTubeEssenceClicked()
    {
        if (InventoryManager.Instance != null)
        {
            // 새로운 아이템 추가
            if (spawnedEssence != null)
            {
                InventoryManager.Instance.AddItem(currentEssenceData, 1);
                Destroy(spawnedEssence);
                spawnedEssence = null;
                Debug.Log("에센스 인벤토리에 추가 완료");
                isMakingEssence = false;
            }
            else
            {
                Debug.Log("에센스 할당 안됨");
            }
        }
        else
        {
            Debug.LogError("InventoryManager.Instance==null");
        }
    }

    // Tube 아이템 삭제
    public void RemoveSpawnedItemd(GameObject itemToRemove)
    {
        if (spawnedItemOnTube.Contains(itemToRemove))
        {
            spawnedItemOnTube.Remove(itemToRemove);
            Destroy(itemToRemove);
        }
    }

    // 아이템 존재 여부 확인
    public bool HasSpawnedItemOnTube()
    {
        // 리스트가 비어있으면 false
        return spawnedItemOnTube != null && spawnedItemOnTube.Count > 0;
    }


    // 아이템 display 생성
    public void DisplayItemOnTube(ItemData itemDisplay, Transform spawnPoint, int indexInTube = 0)
    {
        MaterialData materialData = itemDisplay as MaterialData;
        if (itemDisplay == null || materialData == null || materialData.itemPrefab == null)
        {
            Debug.Log($"DisplayItemOnTue : no item data, {itemDisplay?.name}");
            return;
        }

        // 기존 아이템 파괴
        GameObject displayedItem = Instantiate(materialData.itemPrefab, spawnPoint.position, Quaternion.identity);
        if (displayedItem == null)
        {
            Debug.Log($"displayItemOnTube : Instantiate 실패, {itemDisplay?.name}");
            return;
        }

        displayedItem.transform.SetParent(spawnPoint);
        displayedItem.transform.localPosition = Vector3.zero;
        // 스프라이트 크기 조절

        if (spawnPoint == tubeFuelTransformPos[0]) currentFuel1 = displayedItem;
        else if (spawnPoint == tubeFuelTransformPos[1]) currentFuel2 = displayedItem;
        else if (spawnPoint == tubeFuelTransformPos[2]) currentFuel3 = displayedItem;
        else if (spawnPoint == tubePetalTransformPos[0]) currentPetal = displayedItem;
        else if (spawnPoint == tubeEssenceTransformPos) currentEssence = displayedItem;
    }

    public void ClearAllDisplayedTubeItem()
    {
        if (currentFuel1 != null)
        {
            //Destroy(currentFuel1);
            currentFuel1 = null;
        }
        if (currentFuel2 != null)
        {
            //Destroy(currentFuel2);
            currentFuel2 = null;
        }
        if (currentFuel3 != null)
        {
            //Destroy(currentFuel3);
            currentFuel3 = null;
        }
        if (currentPetal != null)
        {
            //Destroy(currentPetal);
            currentPetal = null;
        }
        if (currentEssence != null)
        {
            //Destroy(currentEssence);
            currentEssence = null;
        }
    }
}

