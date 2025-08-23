using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public class SFXData
    {
        public SFXType type;
        public AudioClip clip;
    }

    [System.Serializable]
    public class BGMData
    {
        public BGMType type;
        public AudioClip clip;
    }

    public List<SFXData> sfxList;
    public List<BGMData> bgmList;

    private Dictionary<SFXType, AudioClip> sfxDict;
    private Dictionary<BGMType, AudioClip> bgmDict;

    private AudioSource sfxSource;
    public AudioSource bgmSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();

        // Dictionary 초기화
        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var data in sfxList) sfxDict[data.type] = data.clip;

        bgmDict = new Dictionary<BGMType, AudioClip>();
        foreach (var data in bgmList) bgmDict[data.type] = data.clip;
    }

    // --- SFX ---
    public void PlaySFX(SFXType type)
    {
        if (sfxDict.ContainsKey(type))
            sfxSource.PlayOneShot(sfxDict[type]);
    }

    // --- BGM ---
    public void PlayBGM(BGMType type, bool loop = true)
    {
        if (bgmDict.ContainsKey(type))
        {
            bgmSource.clip = bgmDict[type];
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    public void StopBGM() => bgmSource.Stop();
}