using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using Unity.VisualScripting;

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
    private Dictionary<string, AudioSource> activeLoops = new Dictionary<string, AudioSource>();

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

    // 스토리 진행을 위함
    // ===========================================
    public void PlaySFX(string typeName)
    {
        if (System.Enum.TryParse(typeName, out SFXType type))
        {
            PlaySFX(type);
        }
        else Debug.LogWarning($"SFXType '{typeName}'을(를) 찾을 수 없습니다.");
    }

    public void PlayLoopSFX(string name, float volume = 1.0f)
    {
        if (activeLoops.ContainsKey(name))
        {
            // 이미 재생 중인 경우 무시
            return;
        }

        if (System.Enum.TryParse(name, out SFXType type) && sfxDict.ContainsKey(type))
        {
            GameObject go = new GameObject($"LoopSFX_{name}");
            go.transform.SetParent(this.transform);

            AudioSource source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.clip = sfxDict[type];
            source.volume = volume;
            source.loop = true;
            source.Play();

            activeLoops.Add(name, source);

        }
        else Debug.LogWarning($"SFXType '{name}'을(를) 찾을 수 없습니다.");
    }

    public void StopLoopSFX(string name)
    {
        if (activeLoops.ContainsKey(name))
        {
            AudioSource source = activeLoops[name];
            if (source != null)
            {
                source.Stop();
                Destroy(source.gameObject);
            }
            activeLoops.Remove(name);
        }
        else
        {
            Debug.LogWarning($"재생 중인 루프 SFX '{name}'이(가) 없습니다.");
        }
    }
    // ===========================================

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

    public BGMType FindByName(string bgmName)
    {
        if (System.Enum.TryParse(bgmName, out BGMType type))
        {
            return type;
        }
        else Debug.LogWarning($"BGMType '{bgmName}'을(를) 찾을 수 없습니다.");
        return default;
    }
}