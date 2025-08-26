using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmSaveService : MonoBehaviour
{
    [SerializeField] private GameObject farmPrefab;
    [SerializeField] private Tilemap farmTilemap;

    public static FarmSaveService Instance { get; private set; }

    void Start()
    {
        GameSave save = SaveManager.Load();
        RestoreFarms(save.farms);
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<FarmSaveData> CreateFarmSnapshot()
    {
        var result = new List<FarmSaveData>();
        // Object.FindObjectsOfType<Farm>() 대체로 FindObjectsByType 을 씀
        foreach (Farm farm in Object.FindObjectsByType<Farm>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            int cropID = (farm.currentCropInstance != null && farm.currentCropInstance.cropData != null) ? farm.currentCropInstance.cropData.id : 0;

            result.Add(new FarmSaveData
            {
                tilePosition = farm.GetTilePosition(),
                isOccpuied = farm.isOccupied,
                isWatered = farm.isWatered,
                cropID = cropID,
                growthStage = farm.currentCropInstance != null ? farm.currentCropInstance.currentStage : 0,
                plantedTime = farm.currentCropInstance != null ? farm.currentCropInstance.plantedUtc : 0
            });
        }
        return result;
    }

    public void RestoreFarms(List<FarmSaveData> farmDataList)
    {
        if (farmDataList == null) return;

        foreach (var data in farmDataList)
        {
            var existingFarm = FindFarmAt(data.tilePosition);
            Farm farm = existingFarm;

            if (farm == null)
            {
                var farmObject = Instantiate(farmPrefab, farmTilemap.CellToWorld(data.tilePosition), Quaternion.identity);
                farm = farmObject.GetComponent<Farm>();
                farm.Init(data.tilePosition, farmTilemap);
            }

            farm.isOccupied = data.isOccpuied;
            farm.isWatered = data.isWatered;

            if (data.cropID != 0)
            {
                SeedData cropData = ItemDataBase.Instance.ResolveItem(data.cropID) as SeedData;
                if (cropData != null)
                {
                    farm.RestoreCrop(cropData, data.growthStage, data.plantedTime);
                }
            }
        }
    }

    private Farm FindFarmAt(Vector3Int tilePos)
    {
        foreach (Farm farm in Object.FindObjectsByType<Farm>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (farm.GetTilePosition() == tilePos)
                return farm;
        }
        return null;
    }
}
