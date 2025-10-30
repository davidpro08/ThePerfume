using System.Collections.Generic;
using UnityEngine;

public class FarmSaveManager : MonoBehaviour
{

    static int farmID = 404;
    // ============ save ===========
    public static void SaveFarms(GameSave save, BuildController controller)
    {
        save.farms ??= new List<FarmSaveData>();
        save.farms.Clear();

        foreach (var kvp in controller.placedObjects)
        {
            GameObject obj = kvp.Value;
            if (obj == null) continue;

            Farm farm = obj.GetComponent<Farm>();
            if (farm != null)
            {
                save.farms.Add(new FarmSaveData
                {
                    gridPosition = kvp.Key,
                    isOccpuied = farm.isOccupied,
                    isWatered = farm.isWatered,
                    seedItemID = farm.currentCropInstance != null ? farm.currentCropInstance.cropData.seed.id : 0,
                    growthStage = farm.currentCropInstance != null ? farm.currentCropInstance.currentStage : 0,
                    cropTimer = farm.currentCropInstance != null ? farm.currentCropInstance.timer : 0
                });
            }
        }
    }

    // ========= Load ===========
    public static void LoadFarms(GameSave save, BuildController controller)
    {
        if (save.farms == null || save.farms.Count == 0) return;

        foreach (var data in save.farms)
        {
            if (!controller.placedObjects.TryGetValue(data.gridPosition, out GameObject farmObj))
            {
                continue;
            }

            var farmComp = farmObj.GetComponent<Farm>();
            if (farmComp == null) return;

            farmComp.Init(data.gridPosition, controller.installationTilemap, farmComp.ItemID);

            farmComp.isOccupied = data.isOccpuied;
            farmComp.isWatered = data.isWatered;
            farmComp.UpdateTile();

            if (data.seedItemID != 0)
            {
                SeedData seed = SaveManager.Instance.GetItemData(data.seedItemID) as SeedData;
                if (seed != null)
                {
                    farmComp.PlantSeed(seed, data.growthStage, data.cropTimer);
                }
            }
        }
    }
}
