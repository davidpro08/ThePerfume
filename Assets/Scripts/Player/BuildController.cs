using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class BuildController : MonoBehaviour
{
    public GameObject tilePreviewPrefab;
    private GameObject currentPreview;

    public Tilemap installationTilemap;

    //public float castDistance = 1.0f;
    //public Transform raycastPoint;
    //public LayerMask layer;

    //public InstallationData farmData;
    public static BuildController Instance { get; private set; }
    public Dictionary<Vector3Int, GameObject> placedObjects = new Dictionary<Vector3Int, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InstallationSaveService.Instance.SetTilemap(installationTilemap);
    }

    private void Update()
    {
        ShowInstallationPreview();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceInstallation();
        }
    }

    void TryPlaceInstallation()
    {
        ItemData equipped = InventoryManager.Instance.EquippedItem();

        if (equipped == null || !(equipped is InstallationData installData))
        {
            return;
        }


        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int gridPos = installationTilemap.WorldToCell(mouseWorldPos);

        // if (placedObjects.ContainsKey(gridPos)) return;

        // 1. Tile 설치 후 Refresh
        if (installData.itemTile != null)
        {
            installationTilemap.SetTile(gridPos, installData.itemTile);
            installationTilemap.RefreshTile(gridPos);
        }

        // 2. Prefab 설치
        if (installData.itemPrefab != null)
        {
            Vector3 worldPos = installationTilemap.GetCellCenterWorld(gridPos);
            GameObject obj = Instantiate(installData.itemPrefab, worldPos, Quaternion.identity);
            obj.AddComponent<InstallationDataHolder>().data = installData;

            placedObjects[gridPos] = obj;
        }

        // 3. 인벤토리 제거 & Snapshot
        InventoryManager.Instance.RemoveItem(equipped, 1);
        InstallationSaveService.Instance.CreateSnapshot(placedObjects);


        // switch (installData.installationType)
        // {
        //     case InstallationType.Farm:
        //         installationTilemap.SetTile(gridPos, installData.itemTile);
        //         installationTilemap.RefreshTile(gridPos);

        //         if (installData.itemPrefab != null)
        //         {
        //             Vector3 worldPos = installationTilemap.GetCellCenterWorld(gridPos);
        //             GameObject farmObj = Instantiate(installData.itemPrefab, worldPos, Quaternion.identity);

        //             Farm farm = farmObj.GetComponent<Farm>();
        //             if (farm != null) farm.Init(gridPos, installationTilemap);

        //             placedObjects[gridPos] = farmObj;

        //             InventoryManager.Instance.RemoveItem(equipped, 1);


        //             InstallationSaveService.Instance.CreateSnapshot(placedObjects);
        //         }
        //         break;
        //     default:
        //         break;
        // }
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

}
