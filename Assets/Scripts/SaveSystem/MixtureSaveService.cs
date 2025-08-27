using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MixtureSaveService : MonoBehaviour
{
    [SerializeField] public Tilemap mixtureTilemap;
    [SerializeField] private GameObject mixturePrefab;

    public static MixtureSaveService Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameSave save = SaveManager.Load();
        if (save.mixture != null) RestoreMixture(save.mixture);
    }

    public MixtureSaveData CreateMixtureSnapshot()
    {
        var result = new MixtureSaveData();
        result = Mixture.Instance.CreateSnapshot();
        return result;
    }

    public void RestoreMixture(MixtureSaveData data)
    {
        if (data == null) return;

        Mixture mixture = Mixture.Instance;
        if (mixture != null)
            mixture.ApplySnapshot(data);
    }
}
