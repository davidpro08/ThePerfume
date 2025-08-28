using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InstallatioinSaveManager
{
    // ========== Save =============
    public static void SaveInstallations(GameSave save, BuildController controller)
    {
        save.installationList ??= new List<InstallationSaveData>();
        save.installationList.Clear();

        foreach (var kvp in controller.placedObjects)
        {
            GameObject obj = kvp.Value;
            if (obj == null) continue;

            // 설치물 아이템 정보 가져오기 (일단 farm만)
            var installation = obj.GetComponent<IInstallation>();
            if (installation == null) continue;

            save.installationList.Add(new InstallationSaveData
            {
                itemID = installation.ItemID,
                tilePosition = kvp.Key,
                sceneName = obj.scene.name
            });
        }
    }

    // =========== Load ============
    public static void LoadInstallations(GameSave save, BuildController controller)
    {
        if (save.installationList == null || save.installationList.Count == 0) return;
        if (controller == null)
        {
            Debug.LogWarning("[InstallationSaveManager] BuildController == null");
            return;
        }

        if (controller.installationTilemap == null)
        {
            Debug.LogWarning("[InstallationSaveManager] BuildController.installationTilemap == null");
            return;
        }

        foreach (var data in save.installationList)
        {
            // DB에서 InstallationData 찾기
            ItemData itemData = SaveManager.Instance.GetItemData(data.itemID);
            if (itemData == null)
            {
                Debug.LogWarning($"ItemData 못 찾음 (ID: {data.itemID})");
                continue;
            }

            InstallationData installData = itemData as InstallationData;
            if (installData == null)
            {
                Debug.LogWarning($"InstallationData 캐스팅 실패 (ID: {data.itemID})");
                continue;
            }

            // 타일 복원
            if (installData.usesTilemap && installData.itemTile != null)
            {
                Debug.Log($"Tilemap: {controller.installationTilemap}");
                controller.installationTilemap.SetTile(data.tilePosition, installData.itemTile);
                controller.installationTilemap.RefreshTile(data.tilePosition);
            }

            // 프리팹 복원
            if (installData.itemPrefab != null)
            {
                Vector3 worldPos = controller.installationTilemap.GetCellCenterWorld(data.tilePosition);
                worldPos.z = 0;

                GameObject obj = Object.Instantiate(installData.itemPrefab, worldPos, Quaternion.identity);

                var installation = obj.GetComponent<IInstallation>();
                if (installation != null)
                {
                    installation.Init(data.tilePosition, controller.installationTilemap, data.itemID);
                }

                controller.RegisterPlacedObject(data.tilePosition, obj);
            }
            Debug.Log($"로드 완료: ID {data.itemID}, 위치 {data.tilePosition}");
        }
    }
}
