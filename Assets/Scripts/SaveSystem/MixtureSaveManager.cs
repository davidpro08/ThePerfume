using UnityEngine;

public class MixtureSaveManager : MonoBehaviour
{
    public static void SaveMixture(GameSave save, MixtureSaveData snapshot)
    {
        save.mixture = snapshot;
    }

    public static MixtureSaveData LoadMixture(GameSave save)
    {
        return save.mixture ?? new MixtureSaveData();
    }
}
