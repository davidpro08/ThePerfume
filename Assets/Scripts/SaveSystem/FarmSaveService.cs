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

    public List<FarmSaveData> CreateFarmSnapshot()
    {
        var result = new List<FarmSaveData>();
        foreach (Farm farm in FindObjectsOfType<Farm>())
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
        foreach (var data in farmDataList)
        {
            var farmObject = Instantiate(farmPrefab, farmTilemap.CellToWorld(data.tilePosition), Quaternion.identity);
            var farm = farmObject.GetComponent<Farm>();
            farm.Init(data.tilePosition, farmTilemap);

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
}
