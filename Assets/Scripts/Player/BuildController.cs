using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildController : MonoBehaviour
{
    public GameObject tilePreviewPrefab;
    private GameObject currentPreview;

    public Tilemap installationTilemap;
    public ItemDataBase itemDB;

    void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetBuildController(this);
            SaveManager.Instance.LoadGame();
        }
    }

    void Awake()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetBuildController(this);

        if (installationTilemap == null)
        {
            Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
            foreach (var tm in tilemaps)
            {
                if (tm.gameObject.name == "Farm_Tilemap" && tm.transform.parent.name == "Grid")
                {
                    installationTilemap = tm;
                    break;
                }
            }

            if (installationTilemap == null)
                Debug.LogWarning("BuildController: Farm_Tilemap 못 찾음");
        }
    }

    private void Update()
    {
        ShowInstallationPreview();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceInstallation();
        }
    }


    public Dictionary<Vector3Int, GameObject> placedObjects = new Dictionary<Vector3Int, GameObject>();
    void TryPlaceInstallation()
    {
        ItemData equipped = InventoryManager.Instance.EquippedItem();

        if (equipped == null) return;

        InstallationData installData = itemDB.ResolveItem(equipped.id) as InstallationData;
        if (installData == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int gridPos = installationTilemap.WorldToCell(mouseWorldPos);

        if (placedObjects.ContainsKey(gridPos)) return;

        switch (installData.installationType)
        {
            case InstallationType.Farm:
            case InstallationType.Distiller:
            case InstallationType.Mixture:
            case InstallationType.Bench:
                installationTilemap.SetTile(gridPos, installData.itemTile);
                installationTilemap.RefreshTile(gridPos);

                if (installData.itemPrefab != null)
                {
                    Vector3 worldPos = installationTilemap.GetCellCenterWorld(gridPos);
                    GameObject farmObj = Instantiate(installData.itemPrefab, worldPos, Quaternion.identity);

                    var installation = farmObj.GetComponent<IInstallation>();
                    if (installation != null) installation.Init(gridPos, installationTilemap, installData.id);

                    placedObjects[gridPos] = farmObj;

                    InventoryManager.Instance.RemoveItem(equipped, 1);

                    SaveManager.Instance.SaveGame();
                }
                break;
            default:
                break;
        }
    }

    void ShowInstallationPreview()
    {
        ItemData equipped = InventoryManager.Instance.EquippedItem();
        if (equipped == null || !(equipped is InstallationData installData))
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int gridPos = installationTilemap.WorldToCell(mouseWorldPos);
        Vector3 worldPos = installationTilemap.GetCellCenterWorld(gridPos);

        if (currentPreview == null && tilePreviewPrefab != null)
        {
            currentPreview = Instantiate(tilePreviewPrefab, worldPos, Quaternion.identity);
        }

        if (currentPreview != null)
        {
            currentPreview.transform.position = worldPos;

            if (placedObjects.ContainsKey(gridPos))
                currentPreview.GetComponent<SpriteRenderer>().color = Color.red * 0.6f;
            else
                currentPreview.GetComponent<SpriteRenderer>().color = Color.white * 0.6f;

        }
    }

    // 설치물 등록 메서드
    public void RegisterPlacedObject(Vector3Int gridPos, GameObject obj)
    {
        placedObjects[gridPos] = obj;
    }

    public Dictionary<Vector3Int, GameObject> PlacedObjects => placedObjects;
}
