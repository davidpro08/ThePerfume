using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildController : MonoBehaviour
{
    public AutoTile farmTile;
    public Tilemap farmTileMap;

    public float castDistance = 1.0f;
    public Transform raycastPoint;
    public LayerMask layer;

    float blockDestroyTime = 0.2f;

    Vector3 direction;
    RaycastHit2D hit;

    bool destroyBlock = false;
    bool placingBlock = false;

    // 만약 화단을 들고 있고 마우스로 클릭하면 설치가 되도록 바꿔야함
    private void FixedUpdate() 
    {
        if(Input.GetKey(KeyCode.F) || Input.GetMouseButtonDown(0))
        {
            RaycastDirection();
        }
    }

    void RaycastDirection()
    {
        if(Input.GetAxis("Horizontal") != 0  || Input.GetAxis("Vertical") != 0)
        {
            direction.x = Input.GetAxis("Horizontal");
            direction.y = Input.GetAxis("Vertical");
        }
        hit = Physics2D.Raycast(raycastPoint.position, direction, castDistance, layer.value);

        Vector2 endPos = raycastPoint.position + direction;

        Debug.DrawLine(raycastPoint.position, endPos, Color.red);
        
        if(Input.GetKey(KeyCode.F))
        {
            if(hit.collider && !destroyBlock)
            {
                destroyBlock = true;

                StartCoroutine(DestroyBlock(hit.collider.gameObject.GetComponent<Tilemap>(), endPos));
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!hit.collider && !placingBlock)
            {
                placingBlock = true;

                StartCoroutine(PlaceBlock(farmTileMap, endPos));
            }
        }
    }

    IEnumerator DestroyBlock(Tilemap map, Vector2 pos)
    {
        yield return new WaitForSeconds(blockDestroyTime);
        
        pos.y = Mathf.Floor(pos.y);
        pos.x = Mathf.Floor(pos.x);

        map.SetTile(new Vector3Int((int)pos.x, (int)pos.y, 0), null);

        destroyBlock = false;
    }

    IEnumerator PlaceBlock(Tilemap map, Vector2 pos)
    {
        yield return new WaitForSeconds(0f);

        pos.y = Mathf.Floor(pos.y);
        pos.x = Mathf.Floor(pos.x);

        map.SetTile(new Vector3Int((int)pos.x, (int)pos.y, 0), farmTile);

        placingBlock = false;
    }
}
