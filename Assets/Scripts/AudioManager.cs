using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource[] sfxAudioSources;

    [Header("Audio Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float bgmVolume = 0.7f;
    [SerializeField] private float sfxVolume = 0.8f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> bgmClipDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClipDict = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioManager()
    {
        // 如果没有分配AudioSource，自动创建
        if (bgmAudioSource == null)
        {
            GameObject bgmObject = new GameObject("BGM AudioSource");
            bgmObject.transform.SetParent(transform);
            bgmAudioSource = bgmObject.AddComponent<AudioSource>();
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
        }

        // 如果没有分配SFX AudioSource数组，创建默认的
        if (sfxAudioSources == null || sfxAudioSources.Length == 0)
        {
            sfxAudioSources = new AudioSource[6]; // 默认6个音效源
            for (int i = 0; i < sfxAudioSources.Length; i++)
            {
                GameObject sfxObject = new GameObject($"SFX AudioSource {i + 1}");
                sfxObject.transform.SetParent(transform);
                sfxAudioSources[i] = sfxObject.AddComponent<AudioSource>();
                sfxAudioSources[i].playOnAwake = false;
                sfxAudioSources[i].loop = false;
            }
        }

        // 初始化音频剪辑字典
        InitializeAudioClips();

        // 设置初始音量
        UpdateVolumes();

        // 开始播放BGM
        PlayBGM("BGM_Lobby");
    }

    private void InitializeAudioClips()
    {
        // 将BGM剪辑添加到字典中
        if (bgmClips != null)
        {
            foreach (var clip in bgmClips)
            {
                if (clip != null && !bgmClipDict.ContainsKey(clip.name))
                {
                    bgmClipDict.Add(clip.name, clip);
                }
            }
        }

        // 将SFX剪辑添加到字典中
        if (sfxClips != null)
        {
            foreach (var clip in sfxClips)
            {
                if (clip != null && !sfxClipDict.ContainsKey(clip.name))
                {
                    sfxClipDict.Add(clip.name, clip);
                }
            }
        }
    }

    #region BGM Methods

    [Header("Fade Settings")]
    [SerializeField] private float fadeTime = 1f; // 淡入淡出时间

    private Coroutine currentFadeCoroutine;

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="clipName">音乐剪辑名称</param>
    /// <param name="loop">是否循环播放（默认true）</param>
    /// <param name="fadeIn">是否淡入（默认true）</param>
    public void PlayBGM(string clipName, bool loop = true, bool fadeIn = true)
    {
        if (bgmClipDict.TryGetValue(clipName, out AudioClip clip))
        {
            PlayBGM(clip, loop, fadeIn);
        }
        else
        {
            Debug.LogWarning($"BGM clip '{clipName}' not found!");
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="clip">音乐剪辑</param>
    /// <param name="loop">是否循环播放（默认true）</param>
    /// <param name="fadeIn">是否淡入（默认true）</param>
    public void PlayBGM(AudioClip clip, bool loop = true, bool fadeIn = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("BGM clip is null!");
            return;
        }

        // 如果当前有BGM在播放，先淡出再播放新的
        if (bgmAudioSource.isPlaying)
        {
            StartCoroutine(CrossFadeBGM(clip, loop, fadeIn));
        }
        else
        {
            // 直接播放新BGM
            StartPlayBGM(clip, loop, fadeIn);
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    /// <param name="fadeOut">是否淡出（默认true）</param>
    public void StopBGM(bool fadeOut = true)
    {
        if (bgmAudioSource.isPlaying)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutBGM());
            }
            else
            {
                bgmAudioSource.Stop();
            }
        }
    }

    /// <summary>
    /// 直接开始播放BGM（内部方法）
    /// </summary>
    private void StartPlayBGM(AudioClip clip, bool loop, bool fadeIn)
    {
        // 停止当前的淡入淡出协程
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // 设置BGM
        bgmAudioSource.clip = clip;
        bgmAudioSource.loop = loop;

        if (fadeIn)
        {
            bgmAudioSource.volume = 0f;
            bgmAudioSource.Play();
            currentFadeCoroutine = StartCoroutine(FadeInBGM());
        }
        else
        {
            bgmAudioSource.volume = bgmVolume * masterVolume;
            bgmAudioSource.Play();
        }
    }

    /// <summary>
    /// 交叉淡入淡出切换BGM
    /// </summary>
    private IEnumerator CrossFadeBGM(AudioClip newClip, bool loop, bool fadeIn)
    {
        // 停止当前的淡入淡出协程
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // 先淡出当前BGM
        yield return StartCoroutine(FadeOutBGM());

        // 再播放新BGM
        StartPlayBGM(newClip, loop, fadeIn);
    }

    /// <summary>
    /// BGM淡入协程
    /// </summary>
    private IEnumerator FadeInBGM()
    {
        float targetVolume = bgmVolume * masterVolume;
        float currentTime = 0f;

        while (currentTime < fadeTime)
        {
            currentTime += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(0f, targetVolume, currentTime / fadeTime);
            yield return null;
        }

        bgmAudioSource.volume = targetVolume;
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// BGM淡出协程
    /// </summary>
    private IEnumerator FadeOutBGM()
    {
        float startVolume = bgmAudioSource.volume;
        float currentTime = 0f;

        while (currentTime < fadeTime)
        {
            currentTime += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
            yield return null;
        }

        bgmAudioSource.volume = 0f;
        bgmAudioSource.Stop();
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBGM()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
        }
    }

    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBGM()
    {
        bgmAudioSource.UnPause();
    }

    /// <summary>
    /// 检查BGM是否正在播放
    /// </summary>
    /// <returns></returns>
    public bool IsBGMPlaying()
    {
        return bgmAudioSource.isPlaying;
    }

    #endregion

    #region SFX Methods

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clipName">音效剪辑名称</param>
    /// <param name="volume">音量（0-1，默认使用设置的音效音量）</param>
    public void PlaySFX(string clipName, float volume = -1f)
    {
        if (sfxClipDict.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySFX(clip, volume);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clip">音效剪辑</param>
    /// <param name="volume">音量（0-1，默认使用设置的音效音量）</param>
    public void PlaySFX(AudioClip clip, float volume = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("SFX clip is null!");
            return;
        }

        // 找到空闲的AudioSource
        AudioSource availableSource = GetAvailableSFXSource();

        if (availableSource != null)
        {
            float finalVolume = volume >= 0 ? volume : sfxVolume * masterVolume;
            availableSource.PlayOneShot(clip, finalVolume);
        }
        else
        {
            Debug.LogWarning("No available SFX AudioSource found!");
        }
    }

    /// <summary>
    /// 在指定位置播放3D音效
    /// </summary>
    /// <param name="clipName">音效剪辑名称</param>
    /// <param name="position">播放位置</param>
    /// <param name="volume">音量</param>
    public void PlaySFXAtPosition(string clipName, Vector3 position, float volume = -1f)
    {
        if (sfxClipDict.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySFXAtPosition(clip, position, volume);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    /// <summary>
    /// 在指定位置播放3D音效
    /// </summary>
    /// <param name="clip">音效剪辑</param>
    /// <param name="position">播放位置</param>
    /// <param name="volume">音量</param>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("SFX clip is null!");
            return;
        }

        float finalVolume = volume >= 0 ? volume : sfxVolume * masterVolume;
        AudioSource.PlayClipAtPoint(clip, position, finalVolume);
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSFX()
    {
        foreach (var source in sfxAudioSources)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        // 寻找空闲的AudioSource
        foreach (var source in sfxAudioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // 如果没有空闲的，返回第一个（会被新音效覆盖）
        return sfxAudioSources[0];
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// 设置主音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetMasterVolume(float volume)
    {
        float oldMasterVolume = masterVolume;
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();

        // 如果BGM正在淡入淡出，也需要按比例调整当前音量
        if (currentFadeCoroutine != null && bgmAudioSource != null && oldMasterVolume > 0)
        {
            // 按音量变化比例调整当前音量
            float ratio = masterVolume / oldMasterVolume;
            bgmAudioSource.volume *= ratio;
        }
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetBGMVolume(float volume)
    {
        float oldTargetVolume = bgmVolume * masterVolume;
        bgmVolume = Mathf.Clamp01(volume);
        float newTargetVolume = bgmVolume * masterVolume;

        UpdateVolumes();

        // 如果BGM正在淡入淡出，也需要调整当前音量
        if (currentFadeCoroutine != null && bgmAudioSource != null && oldTargetVolume > 0)
        {
            // 按比例调整当前音量
            float ratio = newTargetVolume / oldTargetVolume;
            bgmAudioSource.volume *= ratio;
        }
    }

    /// <summary>
    /// 设置SFX音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (bgmAudioSource != null)
        {
            // 如果BGM正在淡入淡出，不直接改变音量，让协程处理
            if (currentFadeCoroutine == null)
            {
                bgmAudioSource.volume = bgmVolume * masterVolume;
            }
            else
            {
                // 如果正在淡入淡出，我们需要重新启动协程以应用新的音量设置
                Debug.Log("BGM正在淡入淡出，重新调整目标音量");
            }
        }

        if (sfxAudioSources != null)
        {
            foreach (var source in sfxAudioSources)
            {
                if (source != null)
                {
                    source.volume = sfxVolume * masterVolume;
                }
            }
        }
    }

    /// <summary>
    /// 强制更新BGM音量（即使在淡入淡出过程中）
    /// </summary>
    public void ForceUpdateBGMVolume()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = bgmVolume * masterVolume;
        }
    }

    /// <summary>
    /// 获取主音量
    /// </summary>
    /// <returns></returns>
    public float GetMasterVolume() => masterVolume;

    /// <summary>
    /// 获取BGM音量
    /// </summary>
    /// <returns></returns>
    public float GetBGMVolume() => bgmVolume;

    /// <summary>
    /// 获取SFX音量
    /// </summary>
    /// <returns></returns>
    public float GetSFXVolume() => sfxVolume;

    #endregion

    #region Utility Methods

    /// <summary>
    /// 动态添加BGM剪辑到字典
    /// </summary>
    /// <param name="clip">音频剪辑</param>
    public void AddBGMClip(AudioClip clip)
    {
        if (clip != null && !bgmClipDict.ContainsKey(clip.name))
        {
            bgmClipDict.Add(clip.name, clip);
        }
    }

    /// <summary>
    /// 动态添加SFX剪辑到字典
    /// </summary>
    /// <param name="clip">音频剪辑</param>
    public void AddSFXClip(AudioClip clip)
    {
        if (clip != null && !sfxClipDict.ContainsKey(clip.name))
        {
            sfxClipDict.Add(clip.name, clip);
        }
    }

    /// <summary>
    /// 检查是否有指定名称的BGM
    /// </summary>
    /// <param name="clipName">剪辑名称</param>
    /// <returns></returns>
    public bool HasBGMClip(string clipName)
    {
        return bgmClipDict.ContainsKey(clipName);
    }

    /// <summary>
    /// 检查是否有指定名称的SFX
    /// </summary>
    /// <param name="clipName">剪辑名称</param>
    /// <returns></returns>
    public bool HasSFXClip(string clipName)
    {
        return sfxClipDict.ContainsKey(clipName);
    }

    #endregion
}