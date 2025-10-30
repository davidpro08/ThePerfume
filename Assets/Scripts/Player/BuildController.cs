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

    [Header("설치 가능 영역")]
    public BoundsInt playableBounds;

    [Header("타일/장애물 체크")]
    [SerializeField] private LayerMask blockedMask;

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

        if (!playableBounds.Contains(gridPos))
        {
            Debug.Log($"[BuildController] 설치 불가 : 범위 밖 {gridPos}");
            return;
        }

        if (placedObjects.ContainsKey(gridPos))
        {
            Debug.Log($"[BuildController] 설치 불가 : 이미 다른 오브젝트 존재");
            return;
        }

        if (IsCellBlocked(gridPos))
        {
            Debug.Log($"[BuildController] 설치 불가 : 장애물에 막힘");
            return;
        }

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

            if (IsCellBlocked(gridPos) || !playableBounds.Contains(gridPos) || placedObjects.ContainsKey(gridPos))
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

    private bool IsCellBlocked(Vector3Int cell)
    {
        Vector3 center = installationTilemap.GetCellCenterWorld(cell);
        Vector2 size = (Vector2)installationTilemap.cellSize * 0.9f;
        float angle = 0f;
        var hit = Physics2D.OverlapBox(center, size, angle, blockedMask);
        return hit != null;
    }
}
