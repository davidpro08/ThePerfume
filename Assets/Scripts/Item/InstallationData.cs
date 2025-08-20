using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Installation", menuName = "Inventory/installation")]
public class InstallationData : ItemData
{
    public GameObject itemPrefab; // 아이템 소환 프리팹
    public TileBase itemTile; // 설치할 타일
    public bool usesTilemap; // 만약 타일맵을 사용하냐, 아니면 프리팹만 설치

    public InstallationType installationType;

    public InstallationData() { }
    public InstallationData(int id, string itemName, string description,
    Sprite itemIcon, ItemType itemType, bool isStackable, int maxStack,
    int nowStack, bool isTradable, int buyPrice, int sellPrice,
    GameObject itemPrefab, InstallationType installationType, TileBase itemTile, bool usesTilemap)
    : base(id, itemName, description, itemIcon, itemType, isStackable, maxStack, nowStack, isTradable, buyPrice, sellPrice)
    {
        this.itemPrefab = itemPrefab;
        this.installationType = installationType;
        this.itemTile = itemTile;
        this.usesTilemap = usesTilemap;
    }
}
