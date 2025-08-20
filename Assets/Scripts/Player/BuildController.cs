using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildController : MonoBehaviour
{
    public Tilemap installationTilemap;

    public float castDistance = 1.0f;
    public Transform raycastPoint;
    public LayerMask layer;

    //float blockDestroyTime = 0.2f;

    private Vector3 direction;
    //RaycastHit2D hit;

    //bool destroyBlock = false;
    private bool placingBlock = false;

    //public InstallationData farmData;

    // 만약 화단을 들고 있고 마우스로 클릭하면 설치가 되도록 바꿔야함
    private void FixedUpdate() 
    {
        if(Input.GetMouseButtonDown(0))
        {
            TryPlaceInstallation();
        }
    }

    void TryPlaceInstallation()
    {
        ItemData equipped = InventoryManager.Instance.EquippedItem();

        if (equipped == null || !(equipped is InstallationData installData)) return;

        if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            direction.x = Input.GetAxis("Horizontal");
            direction.y = Input.GetAxis("Vertical");
        }

        RaycastHit2D hit = Physics2D.Raycast(raycastPoint.position, direction, castDistance, layer.value);
        Vector2 endPos = raycastPoint.position + direction;

        Debug.DrawLine(raycastPoint.position, endPos, Color.red);

        if(!hit.collider && !placingBlock)
        {
            placingBlock = true;
            StartCoroutine(PlaceBlock(installData, endPos));
        }
    }

    //IEnumerator DestroyBlock(Tilemap map, Vector2 pos)
    //{
    //    yield return new WaitForSeconds(blockDestroyTime);

    //    pos.y = Mathf.Floor(pos.y);
    //    pos.x = Mathf.Floor(pos.x);

    //    map.SetTile(new Vector3Int((int)pos.x, (int)pos.y, 0), null);

    //    destroyBlock = false;
    //}

    IEnumerator PlaceBlock(InstallationData installData, Vector2 pos)
    {
        yield return null;

        // 그리드에 맞춰
        pos = new Vector2(Mathf.Floor(pos.x), Mathf.Floor(pos.y));
        Vector3Int gridPos = new Vector3Int((int)pos.x, (int)pos.y, 0);

        switch (installData.installationType)
        {
            case InstallationType.Farm:
                installationTilemap.SetTile(gridPos, installData.itemTile);
                installationTilemap.RefreshTile(gridPos);

                if (installData.itemPrefab != null) { 
                    Vector3 worldPos = installationTilemap.CellToWorld(gridPos) + new Vector3(0.5f, 0.5f, 0);
                    GameObject farmObj = Instantiate(installData.itemPrefab, worldPos, Quaternion.identity);

                    Farm farm = farmObj.GetComponent<Farm>();
                    if (farm != null) farm.Init(gridPos, installationTilemap);
                }
                break;
            default:
                break;
        }
        

        placingBlock = false;
    }


    //IEnumerator PlaceBlock(Tilemap map, Vector2 pos)
    //{
    //    yield return new WaitForSeconds(0f);

    //    pos.y = Mathf.Floor(pos.y);
    //    pos.x = Mathf.Floor(pos.x);

    //    map.SetTile(new Vector3Int((int)pos.x, (int)pos.y, 0), farmTile);

    //    placingBlock = false;
    //}



}
