using UnityEngine;
using UnityEngine.Tilemaps;

public class InstallationBasic : MonoBehaviour, IInstallation
{
    public int ItemID { get; private set; }
    public Vector3Int GridPos { get; private set; }
    private Tilemap tilemap;

    public void Init(Vector3Int gridPos, Tilemap tilemap, int itemID)
    {
        GridPos = gridPos; this.tilemap = tilemap; ItemID = itemID;
    }
}