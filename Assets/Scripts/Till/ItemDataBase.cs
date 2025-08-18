using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DB/ItemDatabase")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemData> items;
    Dictionary<int, ItemData> map; // 아이디, 아이템 데이터

    void OnEnable()
    {
        map = new Dictionary<int, ItemData>();
        foreach (ItemData i in items)
        {
            if (i == null || i.id == 0) continue; // || !i.id.HasValue
            map[i.id] = i;
        }
    }

    public ItemData ResolveItem(int key) => (key != 0 && map != null && map.TryGetValue(key, out ItemData v)) ? v : null;
    public FuelData ResolveFuel(int key) => ResolveItem(key) as FuelData;
    public MaterialData ResolveMaterial(int key) => ResolveItem(key) as MaterialData;
    public EssenceData ResolveEssence(int key) => ResolveItem(key) as EssenceData;
}
