using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DistillerSaveService : MonoBehaviour
{

    [SerializeField] public Tilemap distillerTilemap;
    [SerializeField] private GameObject distillerPrefab;

    public static DistillerSaveService Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameSave save = SaveManager.Load();
        if (save.distillers != null && save.distillers.Count > 0) RestoreDistillers(save.distillers);
    }

    public List<DistillerSaveData> CreateDistillerSapshots()
    {
        var result = new List<DistillerSaveData>();
        foreach (Distiller d in Object.FindObjectsByType<Distiller>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            result.Add(d.SaveSnapshot());
        return result;
    }

    public void RestoreDistillers(List<DistillerSaveData> dataList)
    {
        if (dataList == null) return;

        foreach (var data in dataList)
        {
            Distiller distiller = Distiller.FindByID(data.id);
            if (distiller != null)
                distiller.RebuildFromSave(data);
        }
    }
}
