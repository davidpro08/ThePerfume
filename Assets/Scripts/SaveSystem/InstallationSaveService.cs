using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InstallationSaveService : MonoBehaviour
{
    [Header("설치용 프리팹")]
    [SerializeField] private List<InstallationData> installationPrefabs;
    private Dictionary<int, GameObject> prefabMap;

    [Header("저장 데이터")]
    public List<InstallationSaveData> savedInstallations = new List<InstallationSaveData>();

    [Header("씬 설치용 타일맵")]
    public Tilemap installationTilemap;
    public static InstallationSaveService Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        prefabMap = new Dictionary<int, GameObject>();
        foreach (var data in installationPrefabs)
        {
            if (data != null && data.itemPrefab != null)
                prefabMap[data.id] = data.itemPrefab;
        }
    }

    public void SetTilemap(Tilemap tilemap)
    {
        installationTilemap = tilemap;
        RestoreInstallations();
    }
    public void CreateSnapshot(Dictionary<Vector3Int, GameObject> placedObjects)
    {
        savedInstallations.Clear();

        foreach (var kvp in placedObjects)
        {
            GameObject go = kvp.Value;
            InstallationData instData = go.GetComponent<InstallationDataHolder>().data;
            if (instData == null) continue;

            savedInstallations.Add(new InstallationSaveData
            {
                id = instData.id,
                prefabName = instData.itemPrefab.name,
                tilePosition = kvp.Key,
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            });
        }
    }

    public void RestoreInstallations()
    {
        if (installationTilemap == null || savedInstallations.Count == 0) return;

        foreach (var data in savedInstallations)
        {
            if (!prefabMap.TryGetValue(data.id, out GameObject prefab)) continue;

            Vector3Int gridPos = data.tilePosition;
            Vector3 worldPos = installationTilemap.GetCellCenterWorld(gridPos);

            // Tile 설치
            InstallationData instData = installationPrefabs.Find(d => d.id == data.id);
            if (instData != null && instData.itemTile != null)
            {
                installationTilemap.SetTile(gridPos, instData.itemTile);
            }

            // 프리팹 설치
            GameObject go = Instantiate(prefab, worldPos, Quaternion.identity);
            var holder = go.AddComponent<InstallationDataHolder>();
            holder.data = instData;

            if (BuildController.Instance != null)
                BuildController.Instance.placedObjects[gridPos] = go; // BuildController에도 연결 필요
        }

        installationTilemap.RefreshAllTiles();
    }
}
