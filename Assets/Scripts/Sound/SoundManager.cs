using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup bgmMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        AudioSource[] audioSources = GetComponents<AudioSource>();
        bgmSource = (audioSources.Length > 0) ? audioSources[0] : gameObject.AddComponent<AudioSource>();
        sfxSource = (audioSources.Length > 1) ? audioSources[1] : gameObject.AddComponent<AudioSource>();

        bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var data in sfxList) sfxDict[data.type] = data.clip;

        bgmDict = new Dictionary<BGMType, AudioClip>();
        foreach (var data in bgmList) bgmDict[data.type] = data.clip;
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxDict.ContainsKey(type))
            sfxSource.PlayOneShot(sfxDict[type]);
    }

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